using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using DotnetPackaging;
using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.GitVersion;
using Serilog;
using Zafiro.Mixins;
using Zafiro.Nuke;
using Maybe = CSharpFunctionalExtensions.Maybe;

class Build : NukeBuild
{
    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")] readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("GitHub Authentication Token")] [Secret] readonly string GitHubAuthenticationToken;
    [GitVersion] readonly GitVersion GitVersion;
    [GitRepository] readonly GitRepository Repository;

    public AbsolutePath OutputDirectory = RootDirectory / "output";

    [Solution] Solution Solution;

    public Build()
    {
        Debugger.Launch();
    }

    protected override void OnBuildInitialized()
    {
        Actions = new Actions(Solution, Repository, RootDirectory, GitVersion, Configuration);
    }

    Actions Actions { get; set; }

    Target Clean => td => td
        .Executes(() =>
        {
            OutputDirectory.CreateOrCleanDirectory();
            var absolutePaths = RootDirectory.GlobDirectories("**/bin", "**/obj").Where(a => !((string)a).Contains("build")).ToList();
            Log.Information("Deleting {Dirs}", absolutePaths);
            absolutePaths.DeleteDirectories();
        });

    Target PackAll => td => td
        .OnlyWhenStatic(() => Repository.IsOnMainOrMasterBranch())
        .DependsOn(Clean)
        .Executes(() =>
        {
            Solution.Projects
                .TryFirst(x => x.GetOutputType().Contains("Exe", StringComparison.InvariantCultureIgnoreCase))
                .ToResult("Could not find the executable project")
                .Map(project =>
                {
                    var windowsFiles = Task.FromResult(Actions.CreateZip(project));
                    var options = Options();
                    Debugger.Launch();
                    var linuxAppImageFiles = Actions.CreateAppImages(project, options);

                    var allFiles = new[] { windowsFiles, linuxAppImageFiles, }.Combine();
                    return allFiles
                        .Tap(allFiles => Log.Information("Published @{AllFiles}", allFiles));
                }).TapError(e => throw new ApplicationException(e));
            ;
        });

    Target PublishGitHubRelease => td => td
        .OnlyWhenStatic(() => Repository.IsOnMainOrMasterBranch())
        .DependsOn(Clean)
        .Requires(() => GitHubAuthenticationToken)
        .Executes(() =>
        {
            return Solution.Projects
                .TryFirst(x => x.GetOutputType().Contains("Exe", StringComparison.InvariantCultureIgnoreCase))
                .ToResult("Could not find the executable project")
                .Map(project =>
                {
                    var windowsFiles = Task.FromResult(Actions.CreateZip(project));
                    var options = Options();
                    Debugger.Launch();
                    var linuxAppImageFiles = Actions.CreateAppImages(project, options);

                    var allFiles = new[] { windowsFiles, linuxAppImageFiles, }.Combine();
                    return allFiles
                        .Bind(paths => Actions.CreateGitHubRelease(GitHubAuthenticationToken, paths.Flatten().ToArray()));
                });
        });

    public static int Main() => Execute<Build>(x => x.PublishGitHubRelease);

    Options Options()
    {
        IEnumerable<AdditionalCategory> additionalCategories = [AdditionalCategory.ImageProcessing, AdditionalCategory.FileTools, AdditionalCategory.Photography];

        return new Options
        {
            MainCategory = MainCategory.Utility,
            AdditionalCategories = Maybe.From(additionalCategories),
            AppName = "DeDup",
            Version = GitVersion.MajorMinorPatch,
            Comment = "Remove duplicates pictures from your collection",
            AppId = "com.SuperJMN.DeDup",
            StartupWmClass = "DeDup",
            HomePage = new Uri("https://github.com/SuperJMN/DeDup"),
            Keywords = new List<string>
            {
                "Duplicate",
                "Optimize",
                "Picture",
                "Photos",
                "Images",
                "Shrink",
                "Compact",
            },
            License = "MIT",
            //ScreenshotUrls = Maybe.From(screenShots),
            Summary = "Optimizes your pictures collection by removing duplicates",
        };
    }
}