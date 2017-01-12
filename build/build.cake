#tool nuget:?package=NUnit.Runners.Net4&version=2.6.4
#tool "nuget:?package=GitVersion.CommandLine"
//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var paths = new {
    solution = MakeAbsolute(File("./../src/ZeroLog.sln")).FullPath,
    version = MakeAbsolute(File("./../version.txt")).FullPath,
    assemblyInfo = MakeAbsolute(File("./../src/ZeroLog/Properties/AssemblyInfo.cs")).FullPath,
    output = new {
        build = MakeAbsolute(Directory("./../output/build")).FullPath,
        nuget = MakeAbsolute(Directory("./../output/nuget")).FullPath,
    },
    nuspec = MakeAbsolute(File("./ZeroLog.nuspec")).FullPath,
};

//////////////////////////////////////////////////////////////////////
// HELPERS
//////////////////////////////////////////////////////////////////////

private void Build(string configuration, string outputDirectoryPath)
{
    MSBuild(paths.solution, settings => settings.SetConfiguration(configuration)
                                                .SetPlatformTarget(PlatformTarget.MSIL)
                                                .WithProperty("OutDir", outputDirectoryPath + "/" + configuration));
}

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("UpdateBuildVersionNumber").Does(() =>
{
    if(!AppVeyor.IsRunningOnAppVeyor)
    {
        Information("Not running under AppVeyor");
        return;
    }
    
    Information("Running under AppVeyor");
    var version = System.IO.File.ReadAllText(paths.version);
    var gitVersion = GitVersion();
    version += "-" + gitVersion.Sha;
    Information("Updating AppVeyor build version to " + version);
    AppVeyor.UpdateBuildVersion(version);
});
Task("Clean").Does(() =>
{
    CleanDirectory(paths.output.build);
    CleanDirectory(paths.output.nuget);
});
Task("Restore-NuGet-Packages").Does(() => NuGetRestore(paths.solution));
Task("Create-AssemblyInfo").Does(()=>{
    var version = System.IO.File.ReadAllText(paths.version);
    CreateAssemblyInfo(paths.assemblyInfo, new AssemblyInfoSettings {
            Title = "ZeroLog",
            Product = "ZeroLog",
            Description = "A zero-allocation .NET logging library - https://github.com/Abc-Arbitrage/ZeroLog",
            Copyright = "Copyright © ABC arbitrage 2017",
            Company = "ABC arbitrage",
            Version = version,
            FileVersion = version,
            InternalsVisibleTo = new []{ "ZeroLog.Tests" }
    });
});
Task("Build-Debug").Does(() => Build("Debug", paths.output.build));
Task("Build-Release").Does(() => Build("Release", paths.output.build));
Task("Run-Debug-Unit-Tests").Does(() => NUnit(paths.output.build + "/Debug/*.Tests.exe", new NUnitSettings { Framework = "net-4.6.1", NoResults = true }));
Task("Run-Release-Unit-Tests").Does(() => NUnit(paths.output.build + "/Release/*.Tests.exe", new NUnitSettings { Framework = "net-4.6.1", NoResults = true }));
Task("Nuget-Pack").Does(() => 
{
    var version = System.IO.File.ReadAllText(paths.version);
    NuGetPack(paths.nuspec, new NuGetPackSettings {
        Version = version,
        BasePath = paths.output.build + "/Release",
        OutputDirectory = paths.output.nuget
    });
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Test-Debug")
    .IsDependentOn("UpdateBuildVersionNumber")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore-NuGet-Packages")
    .IsDependentOn("Build-Debug")
    .IsDependentOn("Run-Debug-Unit-Tests");

Task("Test-Release")
    .IsDependentOn("UpdateBuildVersionNumber")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore-NuGet-Packages")
    .IsDependentOn("Build-Release")
    .IsDependentOn("Run-Release-Unit-Tests");

Task("Test-All")
    .IsDependentOn("UpdateBuildVersionNumber")
    .IsDependentOn("Test-Debug")
    .IsDependentOn("Test-Release");

Task("Nuget")
    .IsDependentOn("UpdateBuildVersionNumber")
    .IsDependentOn("Test-All")
    .IsDependentOn("Nuget-Pack")
    .Does(() => {
        var version = System.IO.File.ReadAllText(paths.version);
        Information("   Nuget package is now ready at location: {0}.", paths.output.nuget);
        Warning("   Please remember to create and push a tag based on the currently built version.");
        Information("   You can do so by copying/pasting the following commands:");
        Information("       git tag v{0}", version);
        Information("       git push origin --tags");
    });

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
