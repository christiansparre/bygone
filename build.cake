///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument<string>("target", "Default");
var version = Argument<string>("buildversion", "1.0.0-alpha");
var configuration = Argument<string>("configuration", "Release");
var platform = PlatformTarget.MSIL;

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////

string solution = "./Bygone.sln";
string solutionPath = ".";

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(context =>
{
    Information("Starting build of version: " + version);
});

Teardown(context =>
{
    // Executed AFTER the last task.
    Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASK DEFINITIONS
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    
    Information("Cleaning bin folders");
    CleanDirectories(solutionPath + "/**/bin/**");

    Information("Cleaning obj folders");
    CleanDirectories(solutionPath + "/**/obj/**");

    Information("Cleaning nuget output folder");
    CleanDirectories("./nuget");

});

Task("Restore")
    .Does(() =>
{
    Information("Restoring packages {0}...", solution);
    NuGetRestore(solution);
});

Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .Does(() =>
{
       MSBuild(solution, s => s.SetConfiguration(configuration));
});

Task("BuildNuGetPackages")
    .IsDependentOn("Build")
    .Does(()=> {

        CreateDirectory("./nuget");

        var settings = new NuGetPackSettings { 
            Version = version,
            OutputDirectory = "./nuget",
            ArgumentCustomization = args => args.Append("-Prop configuration=" + configuration) 
            };

        var nuspecFilePaths = GetFiles("./**/Bygone*.nuspec");
        foreach (var path in nuspecFilePaths)
        {
            Information("Building .nuspec file: {0}", path);
            NuGetPack(path, settings);
        }
    });
       
///////////////////////////////////////////////////////////////////////////////
// TARGETS
///////////////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Build");

///////////////////////////////////////////////////////////////////////////////
// EXECUTION
///////////////////////////////////////////////////////////////////////////////

RunTarget(target);