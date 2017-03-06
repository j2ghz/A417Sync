var target = Argument("target", "Default");

Task("Default")
    .IsDependentOn("Build WPF");

Task("Build WPF")
  .Does(() =>
{
  DotNetBuild("./A417Sync.WPF/A417Sync.WPF.csproj", settings =>
    settings.WithTarget("Build"));
});

RunTarget(target);