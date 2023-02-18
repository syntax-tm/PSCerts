using namespace System.Collections
using namespace System.IO
using namespace System.Text
using namespace System.Security.AccessControl
using namespace System.Security.Cryptography.X509Certificates

class TestCert
{
    hidden [bool]$_certLoaded
    hidden [string]$_importedPath

    hidden [X509Certificate2]$Certificate
    hidden [string]$Thumbprint

    [string]$FullName
    [string]$Name
    [string]$Password
    [StoreLocation]$Location
    [StoreName]$Store
    [string]$Extension
    [bool]$Exportable

    TestCert ([string]$fileName, [string]$pass, [StoreLocation]$location, [StoreName]$store, [bool]$isExportable = $true)
    {
        $this.FullName = Join-Path $global:RESOURCES_CERTS_PATH $fileName
        $this.Name = $fileName
        $this.Password = $pass
        $this.Location = $location
        $this.Store = $store
        $this.Exportable = $isExportable

        $this.LoadCertificate()
    }

    hidden [void] LoadCertificate()
    {
        if ($this._certLoaded) { return }

        $this.Validate()
        try
        {
            Write-Verbose "Loading certificate file '$($this.FullName)'..."

            [X509KeyStorageFlags]$storageFlags = 'PersistKeySet'
            if ($this.Exportable) { $storageFlags += 'Exportable' }
            if ($this.Location -eq 'LocalMachine') { $storageFlags += 'MachineKeySet' }

            $securePassword = $this.Password | ConvertTo-SecureString -AsPlainText -Force

            $cert = [X509Certificate2]::new($this.FullName, $securePassword, $storageFlags)
            $this.Certificate = $cert
            $this.Thumbprint = $cert.Thumbprint
            $this._importedPath = "Cert:\$($this.Location)\$($this.Store)$($cert.Thumbprint)"
            $this._certLoaded = $true

            Write-Verbose "Certificate '$($this.Name)' ($($this.Thumbprint)) loaded successfully."
        }
        catch
        {
            Write-Error "An error occurred loading certificate file '$($this.Name)'. $($_.ErrorDetails.Message)" -Category ReadError -ErrorAction Inquire
            Write-Host ""
        }
    }

    hidden [void] Validate()
    {
        if ([string]::IsNullOrWhiteSpace($this.FullName)) { throw [System.ArgumentNullException]::new("FullName")  }
        if ([string]::IsNullOrWhiteSpace($this.Name)) { throw [System.ArgumentNullException]::new("Name")  }
        if ([string]::IsNullOrWhiteSpace($this.Password)) { throw [System.ArgumentNullException]::new("Password")  }
        if ([string]::IsNullOrWhiteSpace($this.Store)) { throw [System.ArgumentNullException]::new("Store")  }

        if (![File]::Exists($this.FullName)) { throw [FileNotFoundException]::new("Certificate file does not exist.", $this.FullName)  }
    }

    [bool] IsImported()
    {
        return Test-Path $this._importedPath
    }

    [void] Import()
    {
        if ($this.IsImported()) { return }

        [X509Store]$certStore = $null

        try
        {
            $certStore = [X509Store]::new($this.Store, $this.Location)
            $certStore.Open('ReadWrite')
            $certStore.Add($this.Certificate)
        }
        finally
        {
            if ($null -ne $certStore -and $certStore.IsOpen) { $certStore.Close() }
        }
    }

    [void] Remove()
    {
        if (-not $this.IsImported()) { return }
        Remove-Item -Path $this._importedPath -Force | Out-Null
    }
}
