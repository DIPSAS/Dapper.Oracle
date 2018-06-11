Framework 4.5.1
properties {  
  $configuration = 'Release'  
}


task default -depends Test

$buildArtifacts = Join-Path $PSScriptRoot "buildartifacts"
$slnFile = Join-Path $PSScriptRoot "src\Dapper.Oracle.sln"
$testProject = Join-Path $PSScriptRoot "src\Tests.Dapper.Oracle\Tests.Dapper.Oracle.csproj"

$testFolder = Join-Path $PSScriptRoot "src\Tests.Dapper.Oracle\bin"


task Init {                     
    dotnet restore $slnFile
    if(Test-Path -Path $buildArtifacts)
    {
        Remove-Item -Path $buildArtifacts -Force -Recurse
    }
}

task Test -depends Compile {
    Exec {        
        dotnet test $testProject                    
    }    
}

task Compile -depends Init {
  Exec {            
    $version = Invoke-GitVersion    
    Write-TargetsFile($version)
    dotnet build $slnfile -c $configuration
  }
}

task Clean {
  Exec { 
    dotnet clean $slnfile 
    Get-ChildItem .\src\Dapper.Oracle -Include obj -Recurse | Remove-Item -Recurse -Force
  }
}

task Package -depends Test {
   Exec {                      
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


function Write-Documentation
{
    Write-Host "Available tasks:Clean,Init,Compile,Test,Package"
    Write-Host "Usage example: build.ps1 -Tasks Test,Package"
}


function Invoke-Gitversion() 
{
        
    if(!(Test-Path -Path ".\packages\GitVersion.CommandLine\tools\GitVersion.exe"))
    {
        if(!(Test-Path -Path .\bin\nuget\nuget.exe))
        {
            New-Item -ItemType Directory -Path .\bin\nuget
            Invoke-WebRequest -Uri "https://www.nuget.org/nuget.exe" -OutFile .\bin\nuget\nuget.exe    
        }

        & .\bin\nuget\nuget.exe install GitVersion.CommandLine -source https://www.nuget.org/api/v2 -o packages -x
    }       

    & .\packages\GitVersion.CommandLine\tools\GitVersion.exe /output json | ConvertFrom-Json
}

function Write-TargetsFile($version)
{
    $code = @"
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace DotNetHelpers
{
    public static class XmlWriter
    {
        private static readonly XNamespace ns = "http://schemas.microsoft.com/developer/msbuild/2003";

        private static readonly string xml = @"<?xml version =""1.0"" encoding=""utf-8""?>
<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
    <PropertyGroup>
        <VersionPrefix></VersionPrefix>
        <VersionSuffix></VersionSuffix>
        <FileVersion></FileVersion>
        <AssemblyVersion></AssemblyVersion>
        <InformationalVersion></InformationalVersion>
        <PackageReleaseNotes>
        </PackageReleaseNotes>
    </PropertyGroup>
</Project>
";

        public static void WriteVersionInfo(string path, string version, string versionSuffix, string releaseNotes)
        {
            XDocument doc = XDocument.Parse(xml);
            doc.Descendants(ns + "VersionPrefix").Single().Value = version;
            doc.Descendants(ns + "VersionSuffix").Single().Value = versionSuffix.StartsWith(".") ? string.Empty : versionSuffix;
            doc.Descendants(ns + "FileVersion").Single().Value = version;
            doc.Descendants(ns + "AssemblyVersion").Single().Value = version.Substring(0, version.IndexOf(".")) + ".0.0.0";
            doc.Descendants(ns + "PackageReleaseNotes").Single().Value = releaseNotes;
            doc.Save(path);
        }
    }

    public static class ReleaseNotes
    {
        public static string GetReleaseNotes(string path, string currentMajor)
        {
            return string.Join(Environment.NewLine, GetReleaseNotesInternal(path, currentMajor));
        }

        private static IEnumerable<string> GetReleaseNotesInternal(string path, string currentMajor)
        {
            var lines = File.ReadAllLines(path);
            var prevMajor = string.Format("### {0}.", int.Parse(currentMajor) - 1);
            foreach (var line in lines)
            {
                if (line.StartsWith(prevMajor))
                {
                    yield break;
                }
                else
                {
                    if (line != "# Releasenotes")
                    {
                        yield return line;
                    }                    
                }
            }

        }
    }

}

"@
    Add-Type -ReferencedAssemblies "System.Xml","System.Xml.Linq" -TypeDefinition $code -Language CSharp | out-null
    
    $path = Join-Path -Path $PSScriptRoot -ChildPath "src\common.generated.targets"
    $relnotespath = Join-Path -Path $PSScriptRoot -ChildPath "releasenotes.md"
    $releaseNotes = [DotNetHelpers.ReleaseNotes]::GetReleaseNotes($relNotesPath,$version.Major)    
    $version
    [DotNetHelpers.XmlWriter]::WriteVersionInfo($path, $version.MajorMinorPatch, $version.PreReleaseTag, $releaseNotes)
}



