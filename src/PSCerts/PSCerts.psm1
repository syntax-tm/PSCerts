$moduleManifestName = "PSCerts.psd1"
$formatFile = "PSCerts.format.ps1xml"
$manifestPath = Join-Path $PSScriptRoot $moduleManifestName
$formatFilePath = Join-Path $PSScriptRoot $formatFile

Import-Module -Name $manifestPath

Update-FormatData -AppendPath $formatFilePath
