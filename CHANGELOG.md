# Release Notes

## 0.0.10

### PSCerts

- Finished adding `Add-SiteBinding` cmdlet
- Updated `Get-CertSummary` return type to `CertSummary`
    - `CertSummary` has an `Items` property which is a `List<CertSummaryItem>`
- Started adding XML documentation to cmdlets

### PSCerts.Tests

- Updated unit tests
- Changed `net7.0` TargetFramework to `net7.0-windows`
- Added `Assert` on `SetUp` to verify they are running as admin
  - Needed to test the `LocalMachine` certificate location as well as being able to locate the private key in various stores

### General

- Added `LICENSE`
- Added `CHANGELOG.md`
