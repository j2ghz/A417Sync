#tool "Squirrel.Windows" 
#addin Cake.Squirrel

var target = Argument("target", "Default");
var directoryWPF = Directory("./A417Sync.WPF");
var version = Argument("version","0.0.0.0");

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

Task("Build WPF Release")
  .IsDependentOn("Restore NuGet")
  .Does(() =>
{
  DotNetBuild("./A417Sync.WPF/A417Sync.WPF.csproj", settings =>
    settings.SetConfiguration("Release").WithTarget("Build"));
});


Task("Restore NuGet")
  .Does(() =>
{
  NuGetRestore("./A417Sync.sln");
});

Task("WPF Package")
    .IsDependentOn("Build WPF Release")
    .Does(() => {
        //nuget pack MyApp.nuspec -Version %(myAssemblyInfo.Version) -Properties Configuration=Release -OutputDirectory $(OutDir) -BasePath $(OutDir)
        var settings = new NuGetPackSettings{
            WorkingDirectory = directoryWPF,
            BasePath = directoryWPF + Directory("bin/Release")
        };
        NuGetPack(directoryWPF + File("A417Sync.WPF.nuspec"), settings);
    });

Task("WPF Installer")
    .IsDependentOn("WPF Package")
    .Does(() => {
        Squirrel(directoryWPF + File("A417Sync.WPF." + version + ".nupkg"));
    });

RunTarget(target);