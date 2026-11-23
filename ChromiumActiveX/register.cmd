@echo off
setlocal
set "REGASM=C:\Windows\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe"
set "REPO=%~dp0.."
for %%I in ("%REPO%") do set "REPO=%%~fI"
set "DLL=%REPO%\bin\ChromiumActiveX.dll"
set "TLB=%REPO%\bin\ChromiumActiveX.tlb"

if not exist "%DLL%" (
  echo Missing %DLL%
  exit /b 1
)

%REGASM% "%DLL%" /codebase /tlb
if errorlevel 1 exit /b %ERRORLEVEL%

set "CODEBASE=file:///%DLL:\=/%"
set "TLBREG=%TLB:\=\\%"
set "TMPREG=%TEMP%\chromiumactivex-system-%RANDOM%.reg"

powershell -NoProfile -Command "$t = Get-Content -Raw -LiteralPath '%~dp0register-system.reg'; $t = $t.Replace('__CODEBASE__','%CODEBASE%').Replace('__TLB__','%TLBREG%'); [IO.File]::WriteAllText('%TMPREG%',$t)"
if errorlevel 1 exit /b %ERRORLEVEL%

%windir%\System32\reg.exe import "%TMPREG%" >nul
set "RC=%ERRORLEVEL%"
del "%TMPREG%" >nul 2>&1
exit /b %RC%
