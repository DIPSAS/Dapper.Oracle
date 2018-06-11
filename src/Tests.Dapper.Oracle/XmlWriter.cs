using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Xunit;

namespace Tests.Dapper.Oracle
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

    public class Test
    {
        [Fact]
        public void Tester()
        {
            var relNotes = ReleaseNotes.GetReleaseNotes(@"c:\github\dapper.oracle\releasenotes.md", "1");
        }
    }
}
