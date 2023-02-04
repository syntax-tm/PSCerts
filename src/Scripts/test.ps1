if (!(Test-Path C:\test))
{
    New-Item -Path C:\test -ItemType Directory
}

$slnRoot = Split-Path $PSScriptRoot
$buildPath = Join-Path $slnRoot "PSCerts\bin\Release"

Copy-Item $buildPath -Destination C:\test\ -Recurse -Force

Import-Module C:\test\Release\PSCerts.psd1 -Force -Verbose
