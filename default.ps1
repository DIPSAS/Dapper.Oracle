Framework 4.5.1
properties {  
  $configuration = 'Release'  
}


task default -depends Test

$buildArtifacts = Join-Path $PSScriptRoot "buildartifacts"
$slnFile = Join-Path $PSScriptRoot "src\Dapper.Oracle.sln"
$testRunner = Join-Path $PSScriptRoot "packages\xunit.runner.console\tools\net452\xunit.console.x86.exe"
$testFolder = Join-Path $PSScriptRoot "src\Tests.Dapper.Oracle\bin"
$paketExe = Join-Path $PSScriptRoot ".paket\paket.exe"

task Init {
    Exec { 
        & $paketExe restore 
        if(Test-Path -Path $buildArtifacts)
        {
            Remove-Item -Path $buildArtifacts -Force -Recurse
        }
    }
}

task Test -depends Compile, Clean {
  Exec {
    $testAssemblies = [System.IO.Path]::Combine($testFolder,$configuration,"Tests.Dapper.Oracle.dll")    
    & $testRunner $testAssemblies -verbose   
  }
}

task Compile -depends Clean,Init {
  Exec {            
    $version = Invoke-GitVersion
    Update-AssemblyInfoFiles -version $version.AssemblySemVer -assemblyInformalVersion $version.InformationalVersion
    msbuild $slnFile /t:Rebuild /p:Configuration=$configuration /v:quiet         
  }
}

task Clean {
  Exec { msbuild $slnFile /t:Clean /p:Configuration=$configuration /v:quiet }
}

task Package -depends Test {
    Exec {
        $version = Invoke-GitVersion   
        Write-PaketTemplateFiles      
        & $paketExe pack $buildArtifacts --verbose --version $version.MajorMinorPatch
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
    $gitVersionPath = Join-Path $PSScriptRoot "packages\GitVersion.CommandLine\tools\GitVersion.exe"
    & $gitVersionPath /output json | ConvertFrom-Json
}

function Write-PaketTemplateFiles()
{
    $relNotes = Get-Content (Join-Path $PSScriptRoot "releasenotes.md")
    Get-ChildItem $PSScriptRoot -Recurse -Filter *.tmpl | % {
        $paketFile = Get-Content $_.FullName
        $paketFile += "`nreleaseNotes`n"
        $relNotes | % {
            $paketFile += "`t$_`n"
        } 
        $newFile = [System.IO.Path]::Combine([System.IO.Path]::GetDirectoryName($_.FullName),"paket.template")
        Set-Content -Value $paketFile -Path $newFile
    }    
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


