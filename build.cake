var target = Argument("target", "Default");

Task("Default")
    .IsDependentOn("Build WPF");

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

RunTarget(target);