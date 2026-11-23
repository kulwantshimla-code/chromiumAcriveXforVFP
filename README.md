# ChromiumBrowser.WebView2 ActiveX Control — Quick Start

A small x86 ActiveX/OLE control that embeds **Chromium via WebView2** for use in **Visual FoxPro 9 (32-bit)**. It replaces the need for the IE-based `ieframe.dll` WebBrowser control with a new ProgID; it does **not** overwrite or reuse any IE identifiers.

---

## 1. What you get

| Item | Value |
| --- | --- |
| ProgID | `ChromiumBrowser.WebView2` |
| CLSID | `{C6D5E4F3-A2B1-4C0D-8E7F-1A2B3C4D5E6F}` |
| LIBID | `{7B8C9D0E-1F2A-4B3C-8D4E-5F6A7B8C9D0E}` |
| Assembly | `ChromiumActiveX, Version=1.0.0.0, Culture=neutral, PublicKeyToken=d89aa6c4567af41f` |
| DLL | `bin/ChromiumActiveX.dll` |
| Type library | `bin/ChromiumActiveX.tlb` |
| Source project | `ChromiumActiveX/ChromiumActiveX.csproj` |

### Repository layout

```
README.md                              this help file
ChromiumActiveX/                       C# source project and registration scripts
ChromiumActiveX/Properties/            strong-name key and assembly metadata
bin/ChromiumActiveX.dll                prebuilt x86 control
bin/ChromiumActiveX.tlb                matching type library
Microsoft.Web.WebView2.*.dll           managed WebView2 assemblies
runtimes/win-x86/native/               WebView2 native loader
sample/test-vfp.prg                    complete VFP9 smoke test
sample/test.html                       sample page loaded by the smoke test
```

---

## 2. Install prerequisites

1. **Visual Studio 2022 Community** with the *.NET desktop development* workload (provides MSBuild and .NET Framework targeting packs).
2. **.NET Framework 4.8 targeting pack** (`C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8`).
3. **WebView2 Runtime (Evergreen)** installed on the machine. In this environment:
   `C:\Program Files (x86)\Microsoft\EdgeWebView\Application\157.0.4341.60\msedgewebview2.exe`
4. **NuGet access** — the build restores `Microsoft.Web.WebView2` version `1.0.4370.26` automatically.
5. **32-bit Visual FoxPro 9** — the control is compiled as **x86** and will not load in a 64-bit host.

For deployment to another machine, copy these files next to `vfp9.exe` (keep the folder structure):

```
ChromiumActiveX.dll
ChromiumActiveX.tlb
Microsoft.Web.WebView2.Core.dll
Microsoft.Web.WebView2.WinForms.dll
Microsoft.Web.WebView2.Wpf.dll
runtimes\win-x86\native\WebView2Loader.dll
```

Then install the WebView2 Runtime and register the control as described in section 4. If the DLL is not at `bin/ChromiumActiveX.dll` on the target machine, edit `$codebase` in `register-user.ps1` (or run `regasm /codebase` from that folder) before registering.

---

## 3. Build

Run:

```bat
cmd /c "ChromiumActiveX/build.cmd"
```

`build.cmd` runs:

```bat
"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" ^
  "ChromiumActiveX/ChromiumActiveX.csproj" ^
  /t:Restore,Build /p:Configuration=Release /p:Platform=x86 /v:minimal
```

Successful output ends with:

```
ChromiumActiveX -> bin/ChromiumActiveX.dll
```

If the **public COM interface** (`IChromiumBrowser.cs`) changes, regenerate the type library as well:

```bat
"C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\TlbExp.exe" ^
  "bin/ChromiumActiveX.dll" /out:"bin/ChromiumActiveX.tlb"
```

The assembly is strong-named with `Properties\ChromiumActiveX.snk`; rebuilds keep the same `PublicKeyToken`, so registration stays valid unless the ProgID/CLSID/version changes.

---

## 4. Register

### Per-user (no administrator required) — recommended for development

Run the supplied script with **32-bit PowerShell**:

```powershell
& "C:\Windows\SysWOW64\WindowsPowerShell\v1.0\powershell.exe" `
  -NoProfile -ExecutionPolicy Bypass `
  -File "ChromiumActiveX/register-user.ps1"
```

Expected output:

```
Registered ChromiumBrowser.WebView2 under HKCU\Software\Classes
```

This writes the ProgID, CLSID, `InprocServer32`, `CodeBase`, TypeLib, OLE-control category, `Control` and `Insertable` keys under `HKCU\Software\Classes`.

### System-wide (requires administrator)

From an **elevated** Command Prompt:

```bat
cmd /c "ChromiumActiveX/register.cmd"
```

`register.cmd` uses `C:\Windows\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe /codebase /tlb`, then imports `register-system.reg` to guarantee full 32-bit and 64-bit HKLM registration (including `Control`, `Insertable` and TypeLib keys). Without elevation it fails with:

```
RegAsm : error RA0000 : An error occurred while writing the registration information to the registry. Administrator permissions are needed to use the selected options. Use an administrator command prompt to complete these tasks.
```

A pre-generated system-wide script is also available at `ChromiumActiveX/register.reg`.

### Verify registration

```powershell
& "C:\Windows\SysWOW64\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -Command ^
  "[Type]::GetTypeFromProgID('ChromiumBrowser.WebView2').FullName"
```

Expected:

```
ChromiumActiveX.ChromiumBrowserControl
```

---

## 5. Run it in Visual FoxPro 9

### Option A — Form Designer (easiest)

1. Open or create a form.
2. On the **Form Controls** toolbar choose **OLE Container Control** and draw it on the form.
3. In the *Insert Object* dialog choose **Insert Control**, then select **ChromiumBrowser.WebView2**.
4. Name the OLE control `ole1` and use it like this (for example in a button click):

```foxpro
THISFORM.ole1.Object.Navigate("https://www.example.com")
```

### Option B — Fully programmatic

```foxpro
* Turn off AutoYield so COM events are delivered reliably
APPLICATION.AutoYield = .F.

oForm = CREATEOBJECT("form")
oForm.Caption = "Chromium WebView2"
oForm.Width  = 900
oForm.Height = 650

oForm.AddObject("web", "olecontrol", "ChromiumBrowser.WebView2")
* Optional: choose a writable WebView2 profile location before Navigate
oForm.web.Object.UserDataFolder = "C:\Temp\ChromiumWebView2"
oForm.web.Left   = 0
oForm.web.Top    = 0
oForm.web.Width  = oForm.Width
oForm.web.Height = oForm.Height - 40
oForm.web.Visible = .T.

oForm.Show()

* Create a tiny local test page, then load it
lcHtml = FULLPATH("sample/test.html")
STRTOFILE('<html><head><title>HelloChromium</title></head><body>OK</body></html>', lcHtml)
oForm.web.Object.Navigate("file:///" + STRTRAN(lcHtml, "\", "/"))
```

Wait until navigation finishes before reading state or running script:

```foxpro
DO WHILE oForm.web.Object.ReadyState < 4
   DOEVENTS
ENDDO

? "Title :", oForm.web.Object.DocumentTitle
? "URL   :", oForm.web.Object.LocationURL

LOCAL lcTitle
lcTitle = oForm.web.Object.ExecuteScript("document.title")
* WebView2 returns a JSON string, so remove the surrounding quotes
lcTitle = STRTRAN(lcTitle, '"', '')
? "Script:", lcTitle

* Keep this modeless form alive until the user closes it
READ EVENTS
```

`READ EVENTS` is required for a form created with `CREATEOBJECT()`; without it the program ends and VFP releases the form as soon as the code above finishes. Call `CLEAR EVENTS` from your close/exit handler, then restore `APPLICATION.AutoYield = .T.`.

A complete runnable version with `READ EVENTS` and form-class cleanup (no `BINDEVENTS()` required) is provided as `sample/test-vfp.prg`.

`ReadyState` values: `0` = not initialized, `1` = loading, `4` = complete.

### Events

Available COM events: `DocumentComplete(url)` and `NavigateError(url, statusCode)`. On VFP 8 or later, bind them with `BINDEVENTS()`:

```foxpro
BINDEVENTS(oForm.web.Object, "DocumentComplete", THIS, "OnDocComplete")

PROCEDURE OnDocComplete
   LPARAMETERS tcUrl
   ? "Loaded:", tcUrl
ENDPROC
```

If your VFP reports `File BINDEVENTS.PRG does not exist`, your runtime predates `BINDEVENTS()`; the smoke test in `test-vfp.prg` avoids it and still works.

---

## 6. API quick reference

| Member | Type | Description |
| --- | --- | --- |
| `Navigate(url)` | method | Navigate to a URL (http/https/file). |
| `Navigate2(url, flags, targetFrameName, postData, headers)` | method | IE-compatible signature; only `url` is required. |
| `Refresh()` | method | Reload the current page. |
| `GoBack()` / `GoForward()` / `Stop()` | method | Browser navigation controls. |
| `SetFocus()` | method | Focus the WebView2 view. |
| `UserDataFolder` | property | Writable WebView2 profile folder; defaults to `%LOCALAPPDATA%\ChromiumActiveX\WebView2UserData`. Set before `Navigate`. |
| `InitializationError` | property | Last WebView2 initialization error, or empty when initialization succeeded. |
| `ExecuteScript(script)` | method | Run JavaScript and return the result as a **JSON string**. |
| `LocationURL` | property | Current URL. |
| `LocationName` | property | Current title/location name. |
| `DocumentTitle` | property | Page `<title>`. |
| `ReadyState` | property | `0`, `1`, or `4`. |
| `Busy` | property | `.T.` while navigating. |
| `DocumentComplete(url)` | event | Fired after a successful navigation. |
| `NavigateError(url, statusCode)` | event | Fired when navigation fails. |

---

## 7. Troubleshooting

| Symptom | Check |
| --- | --- |
| `OLE class not registered` | Run `register-user.ps1` in **32-bit** PowerShell; confirm `HKCU\Software\Classes\ChromiumBrowser.WebView2` exists. |
| Blank/black page | Confirm `runtimes/win-x86/native/WebView2Loader.dll` exists next to the app and the WebView2 Runtime is installed. |
| `ReadyState` stays `0`/`1` | Wait for `4`; check the URL is valid and accessible. |
| `ExecuteScript` appears to hang | Call it only after `ReadyState = 4`; the control now pumps the UI thread while waiting. |
| Events never fire | Set `APPLICATION.AutoYield = .F.` and use `BINDEVENTS()`. |
| Already registered but still fails | Use 32-bit VFP9; the control is x86 only. |
| Not listed in VFP *Insert Control* dialog | Confirm `Control` subkey under `HKCR\CLSID\{C6D5E4F3-A2B1-4C0D-8E7F-1A2B3C4D5E6F}`; re-run the registration script and restart VFP9. |
| `0x80040154 Class not registered` | Re-run `register.cmd` elevated; it imports full HKLM registration. Per-user-only keys can be invisible to VFP in some launch contexts. |
| `Microsoft Edge could not read or write to its directory` | Use this build; it sets `UserDataFolder` under `%LOCALAPPDATA%` instead of under VFP's Program Files path. Optionally set `Object.UserDataFolder` before `Navigate`. |
| Form hangs after `Navigate` / control stays blank | This build pumps messages until WebView2 finishes initializing; check `Object.InitializationError` if it still stalls. |

---

## 8. Build / registration scripts

| Path | Purpose |
| --- | --- |
| `ChromiumActiveX/build.cmd` | Restore + Release x86 build. |
| `ChromiumActiveX/register-user.ps1` | Per-user registration (no admin). |
| `ChromiumActiveX/register.cmd` | System-wide RegAsm registration (admin required). |
| `ChromiumActiveX/register.reg` | RegAsm-generated HKCR registry script. |
| `ChromiumActiveX/register-system.reg` | Full HKLM 32-bit/64-bit registration including `Control`, `Insertable` and TypeLib. |
| `ChromiumActiveX/deploy-webview2-fix.ps1` | Deploys the writable-user-data-folder build after VFP9 is closed. |
| `sample/test-vfp.prg` | Ready-to-run VFP9 smoke test with `READ EVENTS` and close handling. |
| `ChromiumActiveX/Properties\ChromiumActiveX.snk` | Strong-name key. |

Verified smoke test result (x86 COM host, no WinForms synchronization context): local file URL loaded, `ReadyState=4`, `DocumentTitle=HelloChromium`, `InitializationError` empty, and `ExecuteScript("document.title")` returned `"HelloChromium"`.
