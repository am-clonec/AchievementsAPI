var target = Argument("target", "Build");

var workflow = BuildSystem.GitHubActions.Environment.Workflow;
var buildId = workflow.RunNumber;
var tag = workflow.RefType == GitHubActionsRefType.Tag ? workflow.RefName : null;
// Release tags are "vX.Y.Z[-pre]"; MSBuild/NuGet versions must not carry the "v".
var version = tag != null && tag.StartsWith("v") ? tag.Substring(1) : tag;

DotNetMSBuildSettings VersionSettings()
{
    var settings = new DotNetMSBuildSettings();

    if (version != null)
    {
        settings.Version = version;
    }
    else if (buildId != 0)
    {
        settings.VersionSuffix = "ci." + buildId;
    }

    return settings;
}

Task("Build")
    .Does(() =>
{
    DotNetBuild(".", new DotNetBuildSettings
    {
        Configuration = "Release",
        MSBuildSettings = VersionSettings()
    });
});

Task("Pack")
    .IsDependentOn("Build")
    .Does(() =>
{
    DotNetPack("./AchievementsAPI/AchievementsAPI.csproj", new DotNetPackSettings
    {
        Configuration = "Release",
        NoBuild = true,
        OutputDirectory = "./artifacts",
        MSBuildSettings = VersionSettings()
    });
});

RunTarget(target);
