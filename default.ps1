Framework 4.5.1
properties {  
  $configuration = 'Release'  
}


task default -depends Test

$buildArtifacts = Join-Path $PSScriptRoot "buildartifacts"
$slnFile = Join-Path $PSScriptRoot "src\Dapper.Oracle.sln"
$testRunner = Join-Path $PSScriptRoot "packages\tools\dotnet-xunit\tools\net452\xunit.console.x86.exe"
$testFolder = Join-Path $PSScriptRoot "src\Tests.Dapper.Oracle\bin"
$propertiesFile = Join-Path $PSScriptRoot "src\Dapper.Oracle\obj\VersionInfo.g.props"

task Init {                     
    dotnet restore $slnFile
    if(Test-Path -Path $buildArtifacts)
    {
        Remove-Item -Path $buildArtifacts -Force -Recurse
    }
}

task Test -depends Compile {
    Exec {
        pushd .\src\Tests.Dapper.Oracle
    
        dotnet xunit -framework net452 -c $configuration -nobuild -x86
        dotnet xunit -framework netcoreapp2.0 -c $configuration -nobuild --fx-version 2.0.0
    
        popd  
    }    
}

task Compile -depends Init {
  Exec {            
    $version = Invoke-GitVersion
    $versionNumber = [string]::Format("{0}.0",$version.MajorMinorPatch)
    Update-AssemblyInfoFiles -version $versionNumber -assemblyInformalVersion $version.InformationalVersion
    dotnet build $slnfile -c $configuration
  }
}

task Clean {
  Exec { 
    dotnet clean $slnfile 
    Get-ChildItem . -Include obj -Recurse | Remove-Item -Recurse -Force
  }
}

task Package -depends Test {
   Exec { 
        $version = Invoke-GitVersion         
        Write-PaketTemplateFiles                      
        Write-VersionInfo($version)
        dotnet pack $slnFile -c Release --no-build -o ..\..\buildartifacts         
    }
}

task Publish -depends Package {
    Exec {       
        Get-ChildItem -Path $buildArtifacts -Filter *.nupkg | % {
            & $paketExe push $_.FullName --url https://api.nuget.org/v3/index.json
        }
    }
}

task ? -Description "Helper to display task info" {
  Write-Documentation
}

function Invoke-Gitversion() {
    $gitVersionPath = Join-Path $PSScriptRoot "packages\tools\GitVersion.CommandLine\tools\GitVersion.exe"
    & $gitVersionPath /output json | ConvertFrom-Json
}

function Write-PaketTemplateFiles()
{
    $relNotes = Get-Content (Join-Path $PSScriptRoot "releasenotes.md")
    Get-ChildItem $PSScriptRoot -Recurse -Filter *.tmpl | % {
        $paketFile = Get-Content $_.FullName
        $paketFile += "`nreleaseNotes`n"
        $relNotes | % {
            $paketFile += "`t$_"
        } 
        $newFile = [System.IO.Path]::Combine([System.IO.Path]::GetDirectoryName($_.FullName),"paket.template")
        Set-Content -Value $paketFile -Path $newFile
    }    
}

function Write-VersionInfo($version)
{
    $xml = [Xml]@"
<?xml version="1.0" encoding="utf-8" standalone="no"?>
<Project ToolsVersion="14.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
<PropertyGroup>
  <VersionPrefix>1.0.0</VersionPrefix>
  <VersionSuffix></VersionSuffix>
</PropertyGroup>
</Project>
"@
    $xml.Project.PropertyGroup.VersionPrefix = $version.MajorMinorPatch
    $xml.Project.PropertyGroup.VersionSuffix = $version.PreReleaseTag
    $xml.Save($propertiesFile)
}

function Write-Documentation
{
    Write-Host "Available tasks:Clean,Init,Compile,Test,Package"
    Write-Host "Usage example: build.ps1 -Tasks Test,Package"
}


function Update-AssemblyInfoFiles ([string] $version, [string]$assemblyInformalVersion) {
#-------------------------------------------------------------------------------
# Update version numbers of AssemblyInfo.cs
# adapted from: http://www.luisrocha.net/2009/11/setting-assembly-version-with-windows.html
#-------------------------------------------------------------------------------

	if ($version -notmatch "[0-9]+(\.([0-9]+|\*)){1,3}") {
		Write-Error "Version number incorrect format: $version"
	}
	
	$versionPattern = 'AssemblyVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)'
	$versionAssembly = 'AssemblyVersion("' + $version + '")';
	$versionFilePattern = 'AssemblyFileVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)'
    $versionInformalVersionPattern = 'AssemblyInformationalVersion\(".*"\)'
    $versionInformalVersion = 'AssemblyInformationalVersion("' + $assemblyInformalVersion + '")'
	$versionAssemblyFile = 'AssemblyFileVersion("' + $version + '")';
    
    $srcRoot = Join-Path $PSScriptRoot "src"

	Get-ChildItem -r -Path $srcRoot -filter AssemblyInfo.cs | % {
		$filename = $_.fullname
									
		# see http://stackoverflow.com/questions/3057673/powershell-locking-file
		# I am getting really funky locking issues. 
		# The code block below should be:
		#     (get-content $filename) | % {$_ -replace $versionPattern, $version } | set-content $filename

		$tmp = ($filename + ".tmp")
		if (test-path ($tmp)) { remove-item $tmp }		
		(get-content $filename) | 
                % {$_ -replace $versionFilePattern, $versionAssemblyFile } | 
                % {$_ -replace $versionPattern, $versionAssembly } |
                % {$_ -replace $versionInformalVersionPattern, $versionInformalVersion } |  Out-File $tmp
		write-host Updating file AssemblyInfo and AssemblyFileInfo: $filename --> $versionAssembly / $versionAssemblyFile		

		if (test-path ($filename)) { remove-item $filename }
		move-item $tmp $filename -force			
	}
}


