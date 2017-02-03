#tool "nuget:?package=GitVersion.CommandLine"
#addin "Cake.Yaml"

public class ContextInfo
{
    public string NugetVersion { get; set; }
    public string AssemblyVersion { get; set; }
    public GitVersion Git { get; set; }

    public string BuildVersion
    {
        get { return NugetVersion + "-" + Git.Sha; }
    }
}

ContextInfo _versionContext = null;
public ContextInfo VersionContext 
{
    get 
    {
        if(_versionContext == null)
            throw new Exception("The current context has not been read yet. Call ReadContext(FilePath) before accessing the property.");

        return _versionContext;
    }
} 

public ContextInfo ReadContext(FilePath filepath)
{
    _versionContext = DeserializeYamlFromFile<ContextInfo>(filepath);
    try
    {
        _versionContext.Git = GitVersion();
    }
    catch
    {
        _versionContext.Git = new Cake.Common.Tools.GitVersion.GitVersion();
    }
    return _versionContext;
}

public void UpdateAppVeyorBuildVersionNumber()
{   
    var increment = 0;
    while(increment < 10)
    {
        try
        {
            var version = VersionContext.BuildVersion;
            if(increment > 0)
                version += "-" + increment;
            
            AppVeyor.UpdateBuildVersion(version);
            break;
        }
        catch
        {
            increment++;
        }
    }
}
