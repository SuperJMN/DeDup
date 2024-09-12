using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CSharpFunctionalExtensions;
using DotnetPackaging;
using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.GitVersion;
using Serilog;
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
        .Executes(async () =>
        {
            await Solution.Projects
                .TryFirst(x => x.GetOutputType().Contains("Exe", StringComparison.InvariantCultureIgnoreCase))
                .ToResult("Could not find the executable project")
                .Bind(project =>
                {
                    return new DeploymentBuilder(Actions, project)
                        .ForLinux(Options())
                        .ForWindows()
                        .Build()
                        .Tap(allFiles => Log.Information("Published @{AllFiles}", allFiles));
                })
                .TapError(err => throw new ApplicationException(err));;
        });

    Target PublishGitHubRelease => td => td
        .OnlyWhenStatic(() => Repository.IsOnMainOrMasterBranch())
        .DependsOn(Clean)
        .Requires(() => GitHubAuthenticationToken)
        .Executes(async () =>
        {
            await Solution.Projects
                .TryFirst(x => x.GetOutputType().Contains("Exe", StringComparison.InvariantCultureIgnoreCase))
                .ToResult("Could not find the executable project")
                .Bind(project =>
                {
                    return new DeploymentBuilder(Actions, project)
                        .ForLinux(Options())
                        .ForWindows()
                        .Build()
                        .Bind(paths => Actions.CreateGitHubRelease(GitHubAuthenticationToken, paths.ToArray()));
                })
                .TapError(err => throw new ApplicationException(err));
        });

    public static int Main() => Execute<Build>(x => x.PublishGitHubRelease);

    Options Options()
    {
        IEnumerable<AdditionalCategory> additionalCategories = [AdditionalCategory.ImageProcessing, AdditionalCategory.FileTools, AdditionalCategory.Photography];

        return new Options
        {
            MainCategory = MainCategory.Utility,
            AdditionalCategories = Maybe.From(additionalCategories),
            Name = "DeDup",
            Version = GitVersion.MajorMinorPatch,
            Comment = "Remove duplicates pictures from your collection",
            Id = "com.SuperJMN.DeDup",
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