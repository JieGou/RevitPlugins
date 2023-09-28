﻿using System;
using System.Collections.Generic;
using System.Drawing.Imaging;

using Nuke.Common;
using Nuke.Common.IO;

using Serilog;

interface ICreateBundle : IHazPluginName, IHazOutput, IHazTemplate, IHazGitUserName, IHazConfigurations {
    [Parameter] Uri IconUrl => TryGetValue(() => IconUrl);
    [Parameter] string BundleName => TryGetValue(() => BundleName);
    [Parameter] BundleType BundleType => TryGetValue(() => BundleType);
    [Parameter] AbsolutePath BundleOutput => TryGetValue(() => BundleOutput) ?? DefaultOutput;

    Uri DefaultIconUrl => new("https://icons8.com/icon/22361/money-bag");
    string DefaultBundleName => "MyAwesomeBundle";
    BundleType DefaultBundleType => BundleType.InvokeButton;

    AbsolutePath TemplateBundle => TemplateDirectory + BundleType.ExtensionWithDot;
    AbsolutePath BundleDirectory => BundleOutput / BundleName + BundleType.ExtensionWithDot;

    IconSize IconSize => IconSize.Size96;
    string UriIconFormat => "https://img.icons8.com/?size={0}&id={1}&format=png";

    Target CreateBundle => _ => _
        .Requires(() => PluginName)
        .Requires(() => IconUrl)
        .Requires(() => BundleName)
        .Requires(() => BundleType)
        .Requires(() => BundleOutput)
        .OnlyWhenStatic(() =>
            !BundleDirectory.DirectoryExists(), "Bundle directory is exists")
        .OnlyWhenStatic(() =>
            !IconUrl.AbsolutePath.StartsWith(@"https://icons8.com/icon/"), "Bundle icon must be from icons8 site")
        .Executes(async () => {
            AbsolutePath bundleDirectory = NukeBuildExtensions.GetExtensionsPath(BundleDirectory);
            
            Log.Debug("PluginName: {PluginName}", PluginName);
            Log.Debug("TemplateName: {TemplateName}", TemplateName);
            Log.Debug("TemplateBundleDirectory: {TemplateBundleDirectory}", TemplateBundle);

            Log.Debug("IconUrl: {IconUrl}", IconUrl);
            Log.Debug("BundleName: {BundleName}", BundleName);
            Log.Debug("BundleType: {BundleType}", BundleType);
            Log.Debug("BundleDirectory: {BundleDirectory}", BundleDirectory);
            Log.Debug("bundleDirectory: {BundleDirectory}", bundleDirectory);

            bundleDirectory.CreateDirectory();
            CopyDirectory(TemplateBundle, bundleDirectory,
                new Dictionary<string, string>() {
                    {"${{ gen.bundle_name }}", BundleName},
                    {"${{ gen.plugin_name }}", PluginName},
                    {"${{ gen.plugin_command }}", PluginName + "Command"},
                    {"${{ gen.author }}", UserName},
                    {"${{ gen.min_revit_version }}", MinReleaseVersion.ToString()},
                    {"${{ gen.max_revit_version }}", MaxReleaseVersion.ToString()}
                });

            await IconSize.CreateImages(GetImageUri(), bundleDirectory / "icon.png");
        });

    Uri GetImageUri() {
        return new Uri(string.Format(UriIconFormat, IconSize.Size, IconUrl.AbsolutePath.Split('/')[^2]));
    }
}