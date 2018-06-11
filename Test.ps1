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
            doc.Descendants(ns + "VersionSuffix").Single().Value = versionSuffix;
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
                    if (!string.IsNullOrEmpty(line))
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
$ver = Invoke-GitVersion

Write-TargetsFile -version $ver