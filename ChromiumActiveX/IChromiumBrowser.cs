using System.Runtime.InteropServices;

namespace ChromiumActiveX
{
    [ComVisible(true)]
    [Guid("9E4B1F2A-1C3D-4E5F-9A7B-6D8E0F1A2B3C")]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IChromiumBrowser
    {
        [DispId(1)]
        void Navigate(string url);

        [DispId(2)]
        void Navigate2(string url, object flags, object targetFrameName, object postData, object headers);

        [DispId(3)]
        void Refresh();

        [DispId(4)]
        void GoBack();

        [DispId(5)]
        void GoForward();

        [DispId(6)]
        void Stop();

        [DispId(7)]
        string LocationURL { get; }

        [DispId(8)]
        string LocationName { get; }

        [DispId(9)]
        int ReadyState { get; }

        [DispId(10)]
        bool Busy { get; }

        [DispId(11)]
        string DocumentTitle { get; }

        [DispId(12)]
        string ExecuteScript(string script);

        [DispId(13)]
        void SetFocus();

        [DispId(14)]
        string UserDataFolder { get; set; }

        [DispId(15)]
        string InitializationError { get; }
    }

    [ComVisible(true)]
    [Guid("A1B2C3D4-E5F6-4A7B-8C9D-0E1F2A3B4C5D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IChromiumBrowserEvents
    {
        [DispId(1)]
        void DocumentComplete(string url);

        [DispId(2)]
        void NavigateError(string url, int statusCode);
    }

    [ComVisible(true)]
    public delegate void DocumentCompleteEventHandler(string url);

    [ComVisible(true)]
    public delegate void NavigateErrorEventHandler(string url, int statusCode);
}
