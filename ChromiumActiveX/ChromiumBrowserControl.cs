using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace ChromiumActiveX
{
    [ComVisible(true)]
    [Guid("C6D5E4F3-A2B1-4C0D-8E7F-1A2B3C4D5E6F")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComDefaultInterface(typeof(IChromiumBrowser))]
    [ComSourceInterfaces(typeof(IChromiumBrowserEvents))]
    [ProgId("ChromiumBrowser.WebView2")]
    public class ChromiumBrowserControl : UserControl, IChromiumBrowser
    {
        private readonly WebView2 _webView;
        private string _pendingUrl;
        private string _locationUrl = string.Empty;
        private string _userDataFolder = GetDefaultUserDataFolder();
        private string _initError = string.Empty;
        private bool _busy;
        private bool _initializing;

        public event DocumentCompleteEventHandler DocumentComplete;
        public event NavigateErrorEventHandler NavigateError;

        public ChromiumBrowserControl()
        {
            _webView = new WebView2 { Dock = DockStyle.Fill };
            Controls.Add(_webView);
            _webView.NavigationStarting += WebView_NavigationStarting;
            _webView.NavigationCompleted += WebView_NavigationCompleted;
        }

        private static string GetDefaultUserDataFolder()
        {
            string local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(local, "ChromiumActiveX", "WebView2UserData");
        }

        [Browsable(true)]
        [Category("WebView2")]
        [Description("Writable folder used for WebView2 user data. Set before Navigate.")]
        public string UserDataFolder
        {
            get { return _userDataFolder; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    _userDataFolder = Path.GetFullPath(Environment.ExpandEnvironmentVariables(value));
                }
            }
        }

        [Browsable(false)]
        public string InitializationError => _initError;

        [Browsable(false)]
        public string LocationURL
        {
            get
            {
                string source = _webView.Source?.ToString();
                return string.IsNullOrEmpty(source) ? _locationUrl : source;
            }
        }

        [Browsable(false)]
        public string LocationName => DocumentTitle;

        [Browsable(false)]
        public int ReadyState => _busy ? 1 : (_webView.CoreWebView2 == null ? 0 : 4);

        [Browsable(false)]
        public bool Busy => _busy;

        [Browsable(false)]
        public string DocumentTitle => _webView.CoreWebView2?.DocumentTitle ?? string.Empty;

        public void Navigate(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return;
            }

            if (_webView.CoreWebView2 == null)
            {
                _pendingUrl = url;
                EnsureInitialized();
                if (_webView.CoreWebView2 == null)
                {
                    return;
                }
            }

            Uri target;
            if (!Uri.TryCreate(url, UriKind.Absolute, out target))
            {
                Uri baseUri = _webView.Source;
                if (baseUri == null || !Uri.TryCreate(baseUri, url, out target))
                {
                    return;
                }
            }

            RunOnUiThread(() => _webView.Source = target);
        }

        public void Navigate2(string url, object flags, object targetFrameName, object postData, object headers)
        {
            Navigate(url);
        }

        public new void Refresh()
        {
            if (_webView.CoreWebView2 != null)
            {
                RunOnUiThread(() => _webView.Reload());
            }
        }

        public void GoBack()
        {
            if (_webView.CoreWebView2 != null)
            {
                RunOnUiThread(() => _webView.GoBack());
            }
        }

        public void GoForward()
        {
            if (_webView.CoreWebView2 != null)
            {
                RunOnUiThread(() => _webView.GoForward());
            }
        }

        public void Stop()
        {
            if (_webView.CoreWebView2 != null)
            {
                RunOnUiThread(() => _webView.Stop());
            }
        }

        public void SetFocus()
        {
            RunOnUiThread(() => _webView.Focus());
        }

        public string ExecuteScript(string script)
        {
            if (string.IsNullOrEmpty(script) || _webView.CoreWebView2 == null)
            {
                return string.Empty;
            }

            if (!IsHandleCreated || !InvokeRequired)
            {
                System.Threading.Tasks.Task<string> task = _webView.ExecuteScriptAsync(script);
                DateTime deadline = DateTime.UtcNow.AddSeconds(10);
                while (!task.IsCompleted && DateTime.UtcNow < deadline)
                {
                    Application.DoEvents();
                    Thread.Sleep(5);
                }

                if (!task.IsCompleted)
                {
                    throw new TimeoutException("Script execution timed out.");
                }

                if (task.IsFaulted)
                {
                    throw new InvalidOperationException("Script execution failed.", task.Exception);
                }

                return task.GetAwaiter().GetResult() ?? string.Empty;
            }

            string result = string.Empty;
            Exception failure = null;
            using (ManualResetEvent completed = new ManualResetEvent(false))
            {
                BeginInvoke(new Action(() =>
                {
                    _webView.ExecuteScriptAsync(script).ContinueWith(task =>
                    {
                        if (task.IsFaulted)
                        {
                            failure = task.Exception;
                        }
                        else
                        {
                            result = task.Result ?? string.Empty;
                        }
                        completed.Set();
                    });
                }));

                if (!completed.WaitOne(TimeSpan.FromSeconds(10)))
                {
                    throw new TimeoutException("Script execution timed out.");
                }
            }

            if (failure != null)
            {
                throw new InvalidOperationException("Script execution failed.", failure);
            }

            return result;
        }

        private void EnsureInitialized()
        {
            if (_webView.CoreWebView2 != null || _initializing)
            {
                return;
            }

            _initializing = true;
            try
            {
                string folder = _userDataFolder;
                Directory.CreateDirectory(folder);
                System.Threading.Tasks.Task<CoreWebView2Environment> environmentTask =
                    CoreWebView2Environment.CreateAsync(null, folder, null);
                PumpUntilCompleted(environmentTask, 30);
                CoreWebView2Environment environment = environmentTask.GetAwaiter().GetResult();

                System.Threading.Tasks.Task initTask = _webView.EnsureCoreWebView2Async(environment);
                PumpUntilCompleted(initTask, 60);
                _webView.CoreWebView2.DocumentTitleChanged += CoreWebView2_DocumentTitleChanged;
                _initError = string.Empty;

                string url = _pendingUrl;
                if (!string.IsNullOrEmpty(url))
                {
                    _pendingUrl = null;
                    Navigate(url);
                }
            }
            catch (Exception ex)
            {
                _initError = ex.Message;
                _busy = false;
            }
            finally
            {
                _initializing = false;
            }
        }

        private static void PumpUntilCompleted(System.Threading.Tasks.Task task, int timeoutSeconds)
        {
            DateTime deadline = DateTime.UtcNow.AddSeconds(timeoutSeconds);
            while (!task.IsCompleted && DateTime.UtcNow < deadline)
            {
                Application.DoEvents();
                Thread.Sleep(5);
            }

            if (!task.IsCompleted)
            {
                throw new TimeoutException("WebView2 initialization timed out.");
            }

            if (task.IsFaulted)
            {
                throw new InvalidOperationException("WebView2 initialization failed.", task.Exception);
            }
        }

        private void WebView_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            _busy = true;
            _locationUrl = e.Uri ?? _locationUrl;
        }

        private void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            _busy = false;
            Uri source = _webView.Source;
            if (source != null)
            {
                _locationUrl = source.ToString();
            }

            if (e.IsSuccess)
            {
                DocumentComplete?.Invoke(_locationUrl);
            }
            else
            {
                NavigateError?.Invoke(_locationUrl, (int)e.WebErrorStatus);
            }
        }

        private void CoreWebView2_DocumentTitleChanged(object sender, object e)
        {
        }

        private void RunOnUiThread(Action action)
        {
            if (action == null || IsDisposed)
            {
                return;
            }

            if (IsHandleCreated && InvokeRequired)
            {
                BeginInvoke(action);
            }
            else
            {
                action();
            }
        }
    }
}
