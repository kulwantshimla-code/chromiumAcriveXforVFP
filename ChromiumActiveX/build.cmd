@echo off
setlocal
set MSBUILD="C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
%MSBUILD% "%~dp0ChromiumActiveX.csproj" /t:Restore,Build /p:Configuration=Release /p:Platform=x86 /v:minimal
exit /b %ERRORLEVEL%
