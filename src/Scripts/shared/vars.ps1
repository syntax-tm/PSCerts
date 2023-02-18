[string]$global:MODULE_NAME = 'PSCerts'

[string]$global:SCRIPTS_SHARED_PATH = $PSScriptRoot
[string]$global:SCRIPTS_PATH = Split-Path $PSScriptRoot
[string]$global:SLN_ROOT = Split-Path $global:SCRIPTS_PATH
[string]$global:RESOURCES_CERTS_PATH = Join-Path $global:SLN_ROOT "resources\certs"

[string]$global:REPO_ROOT = Split-Path $global:SLN_ROOT
[string]$global:PUBLISH_PATH = Join-Path $global:SLN_ROOT "publish"
[string]$global:TEST_PATH = Join-Path $global:SLN_ROOT "test"

[string]$global:PROJECT_FILE_NAME = "PSCerts.csproj"
[string]$global:PROJECT_PATH = Join-Path $global:SLN_ROOT $global:MODULE_NAME
[string]$global:PROJECT_FILE_PATH = Join-Path $global:PROJECT_PATH $global:PROJECT_FILE_NAME
[string]$global:MODULE_FILE_NAME = "$global:MODULE_NAME.psm1"
[string]$global:MODULE_PATH = Join-Path $global:TEST_PATH $global:MODULE_FILE_NAME
[string]$global:MANIFEST_FILE_NAME = "$global:MODULE_NAME.psd1"
[string]$global:MANIFEST_PATH = Join-Path $global:PUBLISH_PATH $global:MANIFEST_FILE_NAME

[string]$global:VERSION_REGEX = "(ModuleVersion = )'(.*?)'"
[string]$global:RELEASE_NOTES_REGEX = "(ReleaseNotes = )'(.*?)'"
[string]$global:CHANGELOG_FILE_NAME = "CHANGELOG.txt"
[string]$global:CHANGELOG_PATH = Join-Path $global:REPO_ROOT $global:CHANGELOG_FILE_NAME

$global:CONFIGURATION = "Release"
$global:FRAMEWORKS = [hashtable]@{
    coreclr = "netstandard2.0"
    clr     = "net462"
}

$global:MODULE_FILES = @(
    (Join-Path $global:PROJECT_PATH "PSCerts.format.ps1xml")
    (Join-Path $global:PROJECT_PATH $global:MODULE_FILE_NAME)
    (Join-Path $global:PROJECT_PATH $global:MANIFEST_FILE_NAME)
    (Join-Path $global:PROJECT_PATH "init.ps1")
)

$global:BuildStatusCode = 0

$ErrorActionPreference = 'Stop'
