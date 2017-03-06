#tool "Squirrel.Windows" 
#addin Cake.Squirrel

var target = Argument("target", "Default");

Task("Default")
    .IsDependentOn("Build WPF")
    .IsDependentOn("WPF Installer");

Task("Build WPF")
  .IsDependentOn("Restore NuGet")
  .Does(() =>
{
  DotNetBuild("./A417Sync.WPF/A417Sync.WPF.csproj", settings =>
    settings.WithTarget("Build"));
});

Task("Restore NuGet")
  .Does(() =>
{
  NuGetRestore("./A417Sync.sln");
});

Task("WPF Package")
    .Does(() => {
        DotNetBuild("./A417Sync.WPF/A417Sync.WPF.csproj", settings =>
            settings.WithTarget("Package"));
    });

Task("WPF Installer")
    .IsDependentOn("WPF Package")
    .Does(() => {
        Squirrel("./A417Sync.WPF/A417Sync.WPF.nupkg");
    });

RunTarget(target);