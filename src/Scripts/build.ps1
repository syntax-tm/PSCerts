$ErrorActionPreference = 'Stop'

. "$PSScriptRoot\shared\all.ps1"

foreach ($key in $global:FRAMEWORKS.Keys)
{
    $framework = $global:FRAMEWORKS[$key]

    dotnet build "$global:PROJECT_FILE_PATH" -f $framework -c $global:CONFIGURATION --sc true

    if ($LASTEXITCODE -ne 0)
    {
        Write-Host "`nIf you previously ran `"" -ForegroundColor Yellow -NoNewLine
        Write-Host "Import-Module" -ForegroundColor Cyan -NoNewLine
        Write-Host "`" and the error message indicates that the 'file is in use by another process', close and reopen the terminal to remove the reference."  -ForegroundColor Yellow
        Write-Host ""

        $global:BuildStatusCode = -1

        return
    }

    Write-Host "`n$global:PROJECT_FILE_NAME $key build succeeded.`n" -ForegroundColor Green

    Write-Host "Publishing '$global:PROJECT_FILE_NAME'...`n"

    $publishBuildPath = Join-Path $global:PUBLISH_PATH $key

    if (Test-Path $publishBuildPath) {
        Remove-Item -Path $publishBuildPath -Recurse -Force
    }

    dotnet publish "$global:PROJECT_FILE_PATH" -c $global:CONFIGURATION -f $framework --sc -o "$publishBuildPath"

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

foreach ($moduleFile in $global:MODULE_FILES)
{
    Copy-Item -Path $moduleFile -Destination $global:PUBLISH_PATH -Force
}

Write-Host "`nAll $global:PROJECT_FILE_NAME target frameworks successfully built and published." -ForegroundColor Green
