$moduleManifestName = "PSCerts.psd1"
$formatFile = "PSCerts.format.ps1xml"
$initScript = "init.ps1"
$assemblyName = "PSCerts.dll"
$assemblyPath = $null

if ($PSEdition -eq 'Core')
{
    $assemblyPath = Join-Path $PSScriptRoot "coreclr\$assemblyName"
}
else # Desktop
{
    $assemblyPath = Join-Path $PSScriptRoot "clr\$assemblyName"
}

. "$PSScriptRoot\init.ps1"

$manifestPath = Join-Path $PSScriptRoot $moduleManifestName
$formatFilePath = Join-Path $PSScriptRoot $formatFile

Add-Type -Path $assemblyPath

Import-Module -Name $manifestPath

Update-FormatData -AppendPath $formatFilePath
