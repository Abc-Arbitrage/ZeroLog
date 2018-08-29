#tool "nuget:?package=NUnit.ConsoleRunner"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");

var paths = new {
    src = MakeAbsolute(Directory("./../src")).FullPath,
    solution = MakeAbsolute(File("./../src/ZeroLog.sln")).FullPath,
    props = MakeAbsolute(File("./../src/Directory.Build.props")).FullPath,
    testProject = MakeAbsolute(File("./../src/ZeroLog.Tests/ZeroLog.Tests.csproj")).FullPath,
    output = MakeAbsolute(Directory("./../output")).FullPath
};

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Init").Does(() =>
{
    var version = XmlPeek(paths.props, @"/Project/PropertyGroup/Version/text()");
    Information("Version: {0}", version);

    if (AppVeyor.IsRunningOnAppVeyor)
        AppVeyor.UpdateBuildVersion($"{version}-{AppVeyor.Environment.Build.Number}");
});

Task("Clean").Does(() =>
{
    CleanDirectories(GetDirectories(paths.src + "/**/bin/Release"));
    CleanDirectory(paths.output);
});

Task("Restore-NuGet-Packages").Does(() =>
{
    NuGetRestore(paths.solution);
});

Task("Run-Build").Does(() =>
{
    MSBuild(paths.solution, settings => settings
        .WithTarget("Rebuild")
        .SetConfiguration("Release")
        .SetPlatformTarget(PlatformTarget.MSIL)
        .SetVerbosity(Verbosity.Minimal)
    );
});

Task("Run-Tests").Does(() =>
{
    DotNetCoreTest(paths.testProject, new DotNetCoreTestSettings {
        Configuration = "Release",
        NoBuild = true
    });
});

Task("NuGet-Pack").Does(() =>
{
    MSBuild(paths.solution, settings => settings
        .WithTarget("Pack")
        .SetConfiguration("Release")
        .SetPlatformTarget(PlatformTarget.MSIL)
        .SetVerbosity(Verbosity.Minimal)
        .WithProperty("PackageOutputPath", paths.output)
    );
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Build")
    .IsDependentOn("Init")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore-NuGet-Packages")
    .IsDependentOn("Run-Build");

Task("Test")
    .IsDependentOn("Build")
    .IsDependentOn("Run-Tests");

Task("NuGet")
    .IsDependentOn("Test")
    .IsDependentOn("NuGet-Pack");

Task("AppVeyor")
    .IsDependentOn("Build")
    .IsDependentOn("Test")
    .IsDependentOn("NuGet");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
