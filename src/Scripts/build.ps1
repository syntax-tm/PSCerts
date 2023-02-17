param(
    [Parameter(Position = 0)]
    [ValidateSet("Release", "Debug")]
    [Alias("c")]
    [string]$Configuration = "Release"
)

$ErrorActionPreference = 'Stop'

$frameworks = [hashtable]@{
    coreclr = "netstandard2.0"
    clr     = "net462" # "net472"
}

$slnRoot = Split-Path $PSScriptRoot

$moduleFiles = @(
    (Join-Path $slnRoot 'PSCerts\PSCerts.format.ps1xml')
    (Join-Path $slnRoot 'PSCerts\PSCerts.psd1')
    (Join-Path $slnRoot 'PSCerts\PSCerts.psm1')
    (Join-Path $slnRoot 'PSCerts\init.ps1')
)

$projectFileName = "PSCerts.csproj"
$projectFile = Join-Path $slnRoot "PSCerts\$projectFileName"
$publishPath = Join-Path $slnRoot "publish"

$global:BuildStatusCode = 0

$ErrorActionPreference = 'Stop'

foreach ($key in $frameworks.Keys)
{
    $framework = $frameworks[$key]

    dotnet build "$projectFile" -f $framework -c $configuration --sc true

    if ($LASTEXITCODE -ne 0)
    {
        Write-Host "`nIf you previously ran `"" -ForegroundColor Yellow -NoNewLine
        Write-Host "Import-Module" -ForegroundColor Cyan -NoNewLine
        Write-Host "`" and the error message indicates that the 'file is in use by another process', close and reopen the terminal to remove the reference."  -ForegroundColor Yellow
        Write-Host ""

        $global:BuildStatusCode = -1

        return
    }

    Write-Host "`n$projectFileName $key build succeeded.`n" -ForegroundColor Green

    Write-Host "Publishing '$projectFileName'...`n"

    $publishBuildPath = Join-Path $publishPath $key

    if (Test-Path $publishBuildPath) {
        Remove-Item -Path $publishBuildPath -Recurse -Force
    }

    dotnet publish "$projectFile" -c $Configuration -f $framework --sc -o "$publishBuildPath"

    if ($LASTEXITCODE -ne 0)
    {
        Write-Host "`nIf you previously ran `"" -ForegroundColor Yellow -NoNewline
        Write-Host "Import-Module" -ForegroundColor Cyan -NoNewline
        Write-Host "`" and the error message indicates that the 'file is in use by another process', close and reopen the terminal to remove the reference." -ForegroundColor Yellow
        Write-Host ""

        $global:BuildStatusCode = -1

        return
    }

    Get-ChildItem -Path $publishBuildPath -Exclude *.ps1,*.dll,*.deps.json | Remove-Item

    Write-Host "`n$key ($framework) publish was successful." -ForegroundColor Green
}

foreach ($moduleFile in $moduleFiles)
{
    Copy-Item -Path $moduleFile -Destination $publishPath -Force
}

Write-Host "`nPublish completed successfully." -ForegroundColor Green
