@echo off
setlocal
set REGASM="C:\Windows\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe"
%REGASM% "%~dp0..\ChromiumActiveX.dll" /codebase /tlb
if errorlevel 1 exit /b %ERRORLEVEL%
set REG64=%windir%\System32\reg.exe
%REG64% import "%~dp0register-system.reg" >nul
set REG32=%windir%\SysWOW64\reg.exe
if not exist "%REG32%" set REG32=reg.exe
%REG32% add "HKCR\CLSID\{C6D5E4F3-A2B1-4C0D-8E7F-1A2B3C4D5E6F}\Control" /f >nul
%REG32% add "HKCR\CLSID\{C6D5E4F3-A2B1-4C0D-8E7F-1A2B3C4D5E6F}\Insertable" /f >nul
exit /b 0
