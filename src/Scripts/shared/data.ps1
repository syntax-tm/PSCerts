. "$PSScriptRoot\classes\TestCert.ps1"

function script:Get-TestCerts()
{
    param()

    return @(
        [TestCert]::new("PSCerts_01.pfx", "PSCerts_01", "LocalMachine", "My", $true)
    )
}

function Import-TestCerts()
{
    param()

    $testCerts = script:Get-TestCerts

    foreach ($cert in $testCerts)
    {
        $cert.Import()
    }
}

function Remove-TestCerts()
{
    param()

    $testCerts = script:Get-TestCerts

    foreach ($cert in $TestCerts)
    {
        if (-not $cert.IsImported()) { continue }

        $cert.Remove()
    }
}
