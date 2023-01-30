$projectFile = 'PSCerts.csproj'
$configuration = 'Debug'
$outputDir = Join-Path $PSScriptRoot "bin\$configuration"
$assemblyPath = Join-Path $outputDir 'PSCerts.dll'
$modulePath = Join-Path $outputDir 'PSCerts.psd1'

$ErrorActionPreference = 'Stop'

dotnet build --sc true

if ($LASTEXITCODE -ne 0) {

	Write-Host "If you previously ran `"" -ForegroundColor Yellow
	Write-Host "Import-Module" -ForegroundColor Cyan
	Write-Host "`" and the error message indicates that the 'file is in use by another process', you need to remove the module."  -ForegroundColor Yellow
	Write-Host "`nRemove-Module PSCerts" -ForegroundColor Cyan
	Write-Host "`nAlternatively, you can terminate the process indicated that is locking the file." -ForegroundColor Yellow
	Write-Host ""

	return
}

Write-Host "`n$projectFile build succeeded." -ForegroundColor Green
Write-Host "`nFor testing, the module can be imported using the .psd1 file contained in the build directory."
Write-Host "`nImport-Module `"$assemblyPath`"" -ForegroundColor Cyan
Write-Host ""
