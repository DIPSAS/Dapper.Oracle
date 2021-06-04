#tool "nuget:?package=GitVersion.CommandLine"
#load "scripts\utils.cake"

using System.Xml.Linq;
using System.IO;

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
        Configuration = configuration,
        NoBuild = true,
        NoRestore = true
    });
});

// This task is used from appveyor builds, as appveyor build server free edition cannot run linux docker containers.
Task("unit-test")
    .IsDependentOn("build")
    .Does(() =>
{
    DotNetCoreTest(solutionFile,  new DotNetCoreTestSettings 
    {  
        Configuration = configuration,
        NoBuild = true,
        NoRestore = true,
        Filter = "Category!=Integration"
    });
});

Task("signassemblies")
    .IsDependentOn("build")
    .Does(()=> 
    {
        var thumbPrint = EnvironmentVariable("DIPSCodeSigningThumbPrint");
        if(string.IsNullOrEmpty(thumbPrint))
        {
            Information("Skipping codesigning because environment variable for certificate not set.");
            return;
        }        
        var dir = Directory("./bin/release");

        foreach (var item in System.IO.Directory.GetFiles(dir.Path.FullPath,"Dapper.Oracle*.dll",SearchOption.AllDirectories))
        {
            Sign(item, new SignToolSignSettings {
            //TimeStampUri = new Uri("http://sha256timestamp.ws.symantec.com/sha256/timestamp"),  TODO: Returns error, unsure as to why...
            DigestAlgorithm = SignToolDigestAlgorithm.Sha256,
            CertThumbprint = thumbPrint        
            });
        }
    });

// This task is only used from appveyor builds, as appveyor builds cannot sign binaries or nuget packages at the moment.
Task("appveyor-pack")
    .IsDependentOn("unit-test")    
    .Does(()=> 
{    
    var settings = new DotNetCorePackSettings
     {
         Configuration = configuration,
         OutputDirectory = "./artifacts/",
         NoBuild = true,
         NoRestore = true
     };

     DotNetCorePack(solutionFile, settings);
});


Task("pack")
    .IsDependentOn("test")
    .IsDependentOn("signassemblies")
    .Does(()=> 
{    
    var settings = new DotNetCorePackSettings
     {
         Configuration = configuration,
         OutputDirectory = "./artifacts/",
         NoBuild = true,
         NoRestore = true,
         IncludeSymbols = true
     };

     DotNetCorePack(solutionFile, settings);
});

Task("signnuget")
    .IsDependentOn("pack")
    .Does(()=> 
{

    FilePath nugetPath = Context.Tools.Resolve("nuget.exe");    
    Verbose($"Using nuget.exe from {nugetPath.FullPath}");

    var thumbPrint = EnvironmentVariable("DIPSCodeSigningThumbPrint");
        if(string.IsNullOrEmpty(thumbPrint))
        {
            Error("Unable to find thumbprint for codesigning");
            return;
        }        
        var dir = Directory("./artifacts");

        foreach (var item in System.IO.Directory.GetFiles(dir.Path.FullPath,"*.nupkg"))
        {
            var arguments = new ProcessArgumentBuilder()
                    .Append("sign")
                    .Append(item)
                    .Append($"-CertificateFingerprint {thumbPrint}")
                    .Append($"-Timestamper http://sha256timestamp.ws.symantec.com/sha256/timestamp");

            Verbose($"executing nuget.exe {arguments.Render()}");                                
            StartProcess(nugetPath, new ProcessSettings { Arguments = arguments });                 
        }
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











