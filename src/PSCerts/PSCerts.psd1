#
# Module manifest for module 'PSCerts'
#
# Generated by: syntax-tm
#
# Generated on: 1/28/2023
#

@{

# Script module or binary module file associated with this manifest.
RootModule = if ($PSEdition -eq 'Core')
{
    'coreclr\PSCerts.dll'
}
else # Desktop
{
    'clr\PSCerts.dll'
}

# Version number of this module.
ModuleVersion = ''

# Supported PSEditions (PS 5.1+)
# CompatiblePSEditions = @('Desktop','Core')

# ID used to uniquely identify this module
GUID = '49a9cd80-566c-4b57-94e8-4f901894ee33'

# Author of this module
Author = 'syntax-tm'

# Description of the functionality provided by this module
Description = 'Powershell module for managing certificates'

# Minimum version of the PowerShell engine required by this module
PowerShellVersion = '5.0'

# Name of the PowerShell host required by this module
# PowerShellHostName = ''

# Minimum version of the PowerShell host required by this module
# PowerShellHostVersion = ''

# Minimum version of Microsoft .NET Framework required by this module. This prerequisite is valid for the PowerShell Desktop edition only.
DotNetFrameworkVersion = '4.6.2'

# Minimum version of the common language runtime (CLR) required by this module. This prerequisite is valid for the PowerShell Desktop edition only.
# ClrVersion = ''

# Processor architecture (None, X86, Amd64) required by this module
# ProcessorArchitecture = ''

# Modules that must be imported into the global environment prior to importing this module
# RequiredModules = @()

# Assemblies that must be loaded prior to importing this module
RequiredAssemblies = if($PSEdition -eq 'Core')
{
    'coreclr\JsonSubTypes.dll',
    'coreclr\Microsoft.Web.Administration.dll',
    'coreclr\Newtonsoft.Json.dll',
    'coreclr\System.Buffers.dll',
    'coreclr\System.IO.FileSystem.AccessControl.dll'
    'coreclr\System.IO.FileSystem.Primitives.dll',
    'coreclr\System.Memory.dll',
    'coreclr\System.Numerics.Vectors.dll',
    'coreclr\System.Reflection.TypeExtensions.dll',
    'coreclr\System.Runtime.CompilerServices.Unsafe.dll',
    'coreclr\System.Security.AccessControl.dll',
    'coreclr\System.Security.Cryptography.Cng.dll',
    'coreclr\System.Security.Principal.Windows.dll',
    'coreclr\System.Threading.dll'
}
else # Desktop
{
    'clr\JsonSubTypes.dll',
    'clr\Microsoft.Web.Administration.dll',
    'clr\Microsoft.Win32.Registry.dll',
    'clr\Newtonsoft.Json.dll',
    'clr\System.Diagnostics.DiagnosticSource.dll',
    'clr\System.IO.FileSystem.AccessControl.dll',
    'clr\System.Reflection.TypeExtensions.dll',
    'clr\System.Security.AccessControl.dll',
    'clr\System.Security.Cryptography.Cng.dll',
    'clr\System.Security.Principal.Windows.dll',
    'clr\System.ServiceProcess.ServiceController.dll'
}

# Script files (.ps1) that are run in the caller's environment prior to importing this module.
ScriptsToProcess = @(
    'init.ps1'
)

# Type files (.ps1xml) to be loaded when importing this module
# TypesToProcess = @()

# Format files (.ps1xml) to be loaded when importing this module
FormatsToProcess = @(
    'PSCerts.format.ps1xml'
)

# Modules to import as nested modules of the module specified in RootModule/ModuleToProcess
# NestedModules = @()

# Functions to export from this module, for best performance, do not use wildcards and do not delete the entry, use an empty array if there are no functions to export.
FunctionsToExport = @()

# Cmdlets to export from this module, for best performance, do not use wildcards and do not delete the entry, use an empty array if there are no cmdlets to export.
CmdletsToExport = @(
    'Add-CertPermissions',
    'Add-SiteBinding',
    'Get-CertPermissions',
    'Get-CertPrivateKey',
    'Get-CertSummary',
    'Import-Certs',
    'Set-CertFriendlyName'
)

# Variables to export from this module
VariablesToExport = '*'

# Aliases to export from this module, for best performance, do not use wildcards and do not delete the entry, use an empty array if there are no aliases to export.
AliasesToExport = @()

# DSC resources to export from this module
# DscResourcesToExport = @()

# List of all modules packaged with this module
# ModuleList = @()

# List of all files packaged with this module
FileList = @(
    'coreclr\PSCerts.deps.json'
)

# Private data to pass to the module specified in RootModule/ModuleToProcess. This may also contain a PSData hashtable with additional module metadata used by PowerShell.
PrivateData = @{

    PSData = @{

        # Tags applied to this module. These help with module discovery in online galleries.
            Tags   = @('Certificates', 'Key', 'RSA', 'PSEdition_Desktop', 'PSEdition_Core', 'SSL', 'PFX', 'Windows', 'ACL', 'Certificate', 'Permissions', 'IIS')

        # A URL to the license for this module.
        # LicenseUri = ''

        # A URL to the main website for this project.
        ProjectUri = 'https://github.com/syntax-tm/PSCerts'

        # A URL to an icon representing this module.
        IconUri = 'https://github.com/syntax-tm/PSCerts/raw/master/images/PSCerts.ico'

        # ReleaseNotes of this module
        ReleaseNotes = ''

        # Prerelease string of this module
        Prerelease = 'alpha'

        # Flag to indicate whether the module requires explicit user acceptance for install/update/save
        # RequireLicenseAcceptance = $false

        # External dependent modules of this module
        # ExternalModuleDependencies = @()

    } # End of PSData hashtable

} # End of PrivateData hashtable

# HelpInfo URI of this module
# HelpInfoURI = ''

# Default prefix for commands exported from this module. Override the default prefix using Import-Module -Prefix.
# DefaultCommandPrefix = ''

}
