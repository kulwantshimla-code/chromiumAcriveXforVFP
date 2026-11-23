$ErrorActionPreference = 'Stop'
$project = 'D:\rea\ChromiumActiveX'
if (Get-Process vfp9 -ErrorAction SilentlyContinue) {
    Write-Error 'Close VFP9 completely, then run this script again.'
    exit 1
}
Copy-Item -LiteralPath "$project\ChromiumActiveX.fixed.dll" -Destination 'D:\rea\ChromiumActiveX.dll' -Force
Copy-Item -LiteralPath "$project\ChromiumActiveX.fixed.tlb" -Destination 'D:\rea\ChromiumActiveX.tlb' -Force
& 'C:\Windows\SysWOW64\WindowsPowerShell\v1.0\powershell.exe' -NoProfile -ExecutionPolicy Bypass -File "$project\register-user.ps1"
Write-Output 'Deployed WebView2 user-data-folder fix.'
