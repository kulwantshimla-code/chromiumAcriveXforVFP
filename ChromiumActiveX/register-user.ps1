$ErrorActionPreference = 'Stop'

$root = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey('Software\Classes', $true)
$clsid = '{C6D5E4F3-A2B1-4C0D-8E7F-1A2B3C4D5E6F}'
$libid = '{7B8C9D0E-1F2A-4B3C-8D4E-5F6A7B8C9D0E}'
$assembly = 'ChromiumActiveX, Version=1.0.0.0, Culture=neutral, PublicKeyToken=d89aa6c4567af41f'
$codebase = 'file:///D:/rea/ChromiumActiveX.dll'
$tlb = 'D:\rea\ChromiumActiveX.tlb'

function Set-Values($path, $values) {
    $key = $root.CreateSubKey($path)
    foreach ($name in $values.Keys) {
        $key.SetValue($name, $values[$name], [Microsoft.Win32.RegistryValueKind]::String)
    }
    return $key
}

Set-Values 'ChromiumBrowser.WebView2' @{ '' = 'ChromiumActiveX.ChromiumBrowserControl' } | Out-Null
Set-Values 'ChromiumBrowser.WebView2\CLSID' @{ '' = $clsid } | Out-Null

Set-Values "CLSID\$clsid" @{ '' = 'ChromiumActiveX.ChromiumBrowserControl' } | Out-Null
Set-Values "CLSID\$clsid\ProgId" @{ '' = 'ChromiumBrowser.WebView2' } | Out-Null
Set-Values "CLSID\$clsid\TypeLib" @{ '' = $libid } | Out-Null

$server = @{
    '' = 'mscoree.dll'
    'ThreadingModel' = 'Both'
    'Class' = 'ChromiumActiveX.ChromiumBrowserControl'
    'Assembly' = $assembly
    'RuntimeVersion' = 'v4.0.30319'
    'CodeBase' = $codebase
}
Set-Values "CLSID\$clsid\InprocServer32" $server | Out-Null
Set-Values "CLSID\$clsid\InprocServer32\1.0.0.0" $server | Out-Null

Set-Values "CLSID\$clsid\Implemented Categories\{62C8FE65-4EBB-45E7-B440-6E39B2CDBF29}" @{ '' = '' } | Out-Null
Set-Values "CLSID\$clsid\Implemented Categories\{40FC6ED5-9838-11CF-A3DB-080036F12502}" @{ '' = '' } | Out-Null
Set-Values "CLSID\$clsid\Control" @{ '' = '' } | Out-Null
Set-Values "CLSID\$clsid\Insertable" @{ '' = '' } | Out-Null

Set-Values "TypeLib\$libid\1.0" @{ '' = 'ChromiumActiveX 1.0' } | Out-Null
Set-Values "TypeLib\$libid\1.0\FLAGS" @{ '' = '0' } | Out-Null
Set-Values "TypeLib\$libid\1.0\0\win32" @{ '' = $tlb } | Out-Null

$root.Close()
Write-Output 'Registered ChromiumBrowser.WebView2 under HKCU\Software\Classes'
