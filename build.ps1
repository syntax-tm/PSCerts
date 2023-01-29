$projectFile = 'PSCerts.csproj'
$configuration = 'Debug'
$outputDir = Join-Path $PSScriptRoot "bin\$configuration"
$assemblyPath = Join-Path $outputDir 'PSCerts.dll'
$modulePath = Join-Path $outputDir 'PSCerts.psd1'

$ErrorActionPreference = 'Stop'

dotnet build -v m --sc true

if ($LASTEXITCODE -ne 0) {

	Write-Host @"

If the file is in use by another process, make sure you removed the module if it was imported using:

Remove-Module -Name PSCerts

"@ -ForegroundColor Yellow

	return
}

$message = @"

PSCerts module build succeeded.

For testing, the module can be imported using the .psd1 file contained in the build directory. For example:

Import-Module "$modulePath"

"@

Write-Host $message -ForegroundColor Green
