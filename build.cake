#tool "nuget:?package=GitVersion.CommandLine"
#load "changelog.cake"

using System.Xml.Linq;

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "default");
var configuration = Argument("configuration", "Release");
GitVersion version;

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////


var solutionFile = File("./src/Dapper.Oracle.sln");
XNamespace ns = "http://schemas.microsoft.com/developer/msbuild/2003";


//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("clean")
    .Does(() =>
{
    DotNetCoreClean(solutionFile);
    DeleteFiles("./artifacts/*.nupkg");
    
});

Task("init")
    .IsDependentOn("clean")
    .Does(() =>
{
    DotNetCoreRestore(solutionFile);
    version = GitVersion(new GitVersionSettings());
    WriteVersionInformationToMsBuildFile(version, "./releasenotes.md");

});

Task("build")
    .IsDependentOn("init")
    .Does(() =>
{
    DotNetCoreBuild(solutionFile, new DotNetCoreBuildSettings
     {         
         Configuration = configuration         
     });
});

Task("test")
    .IsDependentOn("build")
    .Does(() =>
{
    DotNetCoreTest(solutionFile,  new DotNetCoreTestSettings 
    {  
        Configuration = configuration
    });
});

Task("pack")
    .IsDependentOn("test")
    .Does(()=> 
{    
    var settings = new DotNetCorePackSettings
     {
         Configuration = "Release",
         OutputDirectory = "./artifacts/"
     };

     DotNetCorePack(solutionFile, settings);
});



//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("default")
    .IsDependentOn("test");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);






private void WriteImportOfVersionInfoToMsBuildFile()
{
    WriteToMsBuildXml("./src/directory.build.props", root => {
        root.AddFirst(
            new XElement("Import",
                new XAttribute("Project", "$(MSBuildThisFileDirectory)versioninfo.props"),
                new XAttribute("Condition", "exists('$(MSBuildThisFileDirectory)versioninfo.props')")));
    });
}

private void WriteToMsBuildXml(FilePath file, Action<XElement> add)
{
    XElement root;    

    if (!FileExists(file))
    {
        Information("File not found, creating new");
        root = new XElement(ns + "Project");
    }
    else
    {
        Information("Using existing file");
        root = XElement.Load(file.FullPath);
    }

    add(root);
    root.Save(file.FullPath, SaveOptions.None);
}


private FilePath GetVersionInfoFile()
{
    return File("./src/versioninfo.props");
}

private void WriteVersionInformationToMsBuildFile(GitVersion version, FilePath changelogFile)
{
    var log = ChangeLog.Parse(changelogFile.FullPath);
    var releaseNotes = log?.GetVersion(version.MajorMinorPatch)?.Body;

    // Ignore if version is prerelease
    if (string.IsNullOrEmpty(releaseNotes))
    {
        if (string.IsNullOrEmpty(version.PreReleaseTag))
        {
            throw new Exception($"Release notes for version {version.MajorMinorPatch} is missing");
        }

        Warning($"Missing release notes for version {version.MajorMinorPatch} but ignoring it because this is a prelease version");
    }

    var file = GetVersionInfoFile();

    WriteToMsBuildXml(file, root => {
        Information($"Writing version information [{version.NuGetVersionV2}] to {file}");

        root.Descendants(ns+"PropertyGroup").Remove();
        var pg = new XElement(ns + "PropertyGroup");
        pg.Add(new XElement(ns + "Version", version.AssemblySemFileVer));
        pg.Add(new XElement(ns + "FileVersion", version.AssemblySemFileVer));
        pg.Add(new XElement(ns + "AssemblyVersion", version.AssemblySemVer));
        pg.Add(new XElement(ns + "InformationalVersion", version.InformationalVersion));
        pg.Add(new XElement(ns + "PackageReleaseNotes", releaseNotes));

        root.Add(pg);                        
    });
}




