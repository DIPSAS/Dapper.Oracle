
#requires -version 5.0

param(        
    [Parameter(mandatory=$true,ValueFromPipeline=$false)]
    [String[]]$Tasks,
    [Parameter(mandatory=$false,ValueFromPipeline=$false)]
    [String]$Configuration="Release"    
)

if(!(Get-Module -ListAvailable -name psake))
{ 
    Find-Module psake | Install-Module -Scope CurrentUser -SkipPublisherCheck
}

if(!(Get-Module -ListAvailable -name VSSetup))
{ 
    Install-Module VSSetup -Scope CurrentUser
}

Import-Module psake

Invoke-psake (Join-Path $PSScriptRoot "default.ps1") -properties @{"configuration" = $Configuration} $Tasks