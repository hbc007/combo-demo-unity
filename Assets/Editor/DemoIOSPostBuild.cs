﻿#if UNITY_IOS
using UnityEditor.Build;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions;
using UnityEditor.Build.Reporting;
using System.Collections.Generic;

public class DemoIOSPostBuild : IPostprocessBuildWithReport
{
    public int callbackOrder { get { return 1; } }

    public void CreateDirectory(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }

    private static readonly string ComboSDKFrameworks = "ComboSDKFrameworks";

    public void OnPostprocessBuild(BuildReport report)
    {
        var szFrameworkPath = System.Environment.GetEnvironmentVariable("FRAMEWORK_PATH") ?? "Frameworks";
        if (report.summary.platform == BuildTarget.iOS)
        {
            string projectPath = report.summary.outputPath + "/Unity-iPhone.xcodeproj/project.pbxproj";
            PBXProject pbxProject = new PBXProject();
            pbxProject.ReadFromFile(projectPath);

            string unityMainTargetGuid = pbxProject.GetUnityMainTargetGuid();
            string unityFrameworkTargetGuid = pbxProject.GetUnityFrameworkTargetGuid();
            var unityMainFrameworksBuildPhase = pbxProject.GetFrameworksBuildPhaseByTarget(unityMainTargetGuid);
            var unityFrameworkBuildPhase = pbxProject.GetFrameworksBuildPhaseByTarget(unityFrameworkTargetGuid);

            // Add Frameworks
            AddFrameworks(szFrameworkPath, report, pbxProject, unityMainTargetGuid, unityMainFrameworksBuildPhase, unityFrameworkTargetGuid, unityFrameworkBuildPhase);

            // Build Setting
            SetBuildProperty(pbxProject, unityMainTargetGuid, unityFrameworkTargetGuid);

            // ComboSDK.json
            CopyAndAddComboSDKJson(report, pbxProject, unityMainTargetGuid);

            // Add Sign In With Apple Capability
            AddAppleSignInCapability(report, pbxProject, unityMainTargetGuid);

            pbxProject.WriteToFile(projectPath);

            // Info.plist
            UpdatePListFile(report);

            Debug.Log($"[Demo] PostBuild iOS init xcodeproj finish!");
        }
    }

    private void AddFrameworks(string szFrameworkPath, BuildReport report, PBXProject pbxProject,
        string mainTargetGuid, string mainFrameworksBuildPhase, string frameworkTargetGuid, string frameworkBuildPhase)
    {
        // 扫描一级目录
        var levelOneDirectories = Directory.GetDirectories(szFrameworkPath);
        var frameworks = new Dictionary<string, List<string>>();

        foreach (var levelOneDirectory in levelOneDirectories)
        {
            var levelOneDirectoryName = Path.GetFileName(levelOneDirectory);

            // 筛选一级目录 .xcframework 和 .framework 目录
            if (levelOneDirectoryName.EndsWith(".xcframework") || levelOneDirectoryName.EndsWith(".framework"))
            {
                if (!frameworks.ContainsKey(szFrameworkPath))
                {
                    frameworks[szFrameworkPath] = new List<string>();
                }
                frameworks[szFrameworkPath].Add(levelOneDirectoryName);
            }
            else
            {
                // 扫描第二级目录
                var levelTwoDirectories = Directory.GetDirectories(levelOneDirectory);

                // 通过名称筛选出 .xcframework 和 .framework 目录
                var frameworkDirectories = levelTwoDirectories.Where(directory =>
                    directory.EndsWith(".xcframework") || directory.EndsWith(".framework")).ToArray();

                if(frameworkDirectories.Length > 0)
                {
                    frameworks[levelOneDirectory] = frameworkDirectories.Select(Path.GetFileName).ToList();
                }
            }
        }

        foreach (var pair in frameworks)
        {
            foreach (var frameworkName in pair.Value)
            {
                string destPath = Path.Combine(report.summary.outputPath, ComboSDKFrameworks, frameworkName);
                Builder.DirectoryCopy(Path.Combine(pair.Key, frameworkName), destPath);

                string fileGuid = pbxProject.AddFile(destPath, $"{ComboSDKFrameworks}/{frameworkName}", PBXSourceTree.Sdk);
                if (fileGuid != null)
                {
                    pbxProject.AddFileToEmbedFrameworks(mainTargetGuid, fileGuid);
                    pbxProject.AddFileToBuildSection(mainTargetGuid, mainFrameworksBuildPhase, fileGuid);
                    pbxProject.AddFileToBuildSection(frameworkTargetGuid, frameworkBuildPhase, fileGuid);
                }
                else
                {
                    Debug.Log($"{frameworkName} file not found!");
                }
            }
        }
    }

    private void SetBuildProperty(PBXProject pbxProject, string mainTargetGuid, string unityFrameworkTargetGuid)
    {
        // Sign
        pbxProject.SetBuildProperty(mainTargetGuid, "PRODUCT_BUNDLE_IDENTIFIER", "com.ksDemo.omni");
        pbxProject.SetBuildProperty(mainTargetGuid, "CODE_SIGN_STYLE", "Manual");
        pbxProject.SetBuildProperty(mainTargetGuid, "CODE_SIGN_IDENTITY", "Apple Development: TingTing Liu (QWVQYB57WJ)");
        pbxProject.SetBuildProperty(mainTargetGuid, "PROVISIONING_PROFILE_SPECIFIER", "dev_provision");
        pbxProject.SetBuildProperty(mainTargetGuid, "DEVELOPMENT_TEAM", "SP537S8Q2J");
        // Framework search path
        pbxProject.SetBuildProperty(mainTargetGuid, "FRAMEWORK_SEARCH_PATHS", $"$(PROJECT_DIR)/{ComboSDKFrameworks}");
        pbxProject.SetBuildProperty(unityFrameworkTargetGuid, "FRAMEWORK_SEARCH_PATHS", $"$(PROJECT_DIR)/{ComboSDKFrameworks}");
    }

    private void AddAppleSignInCapability(BuildReport report, PBXProject pbxProject, string mainTargetGuid)
    {
        var entitlementsPath = $"{report.summary.outputPath}/Unity-iPhone/Unity-iPhone.entitlements";
        var entitlements = new PlistDocument();
        var array = entitlements.root.CreateArray("com.apple.developer.applesignin");
        array.AddString("Default");
        File.WriteAllText(entitlementsPath, entitlements.WriteToString());
        var relativeEntitlementsPath = "Unity-iPhone/Unity-iPhone.entitlements";
        pbxProject.AddFile(entitlementsPath, relativeEntitlementsPath);
        pbxProject.AddCapability(mainTargetGuid, PBXCapabilityType.SignInWithApple, relativeEntitlementsPath);
    }

    private void CopyAndAddComboSDKJson(BuildReport report, PBXProject pbxProject, string mainTargetGuid)
    {
        var jsonFilePath = $"{Application.dataPath}/Plugins/iOS/ComboSDK.json";
        string destJsonFilePath = Path.Combine(report.summary.outputPath, ComboSDKFrameworks, "ComboSDK.json");

        Builder.CopyFile(jsonFilePath, destJsonFilePath);

        var guid = pbxProject.AddFile(destJsonFilePath, $"{ComboSDKFrameworks}/ComboSDK.json", PBXSourceTree.Source);
        pbxProject.AddFileToBuild(mainTargetGuid, guid);
    }

    private void UpdatePListFile(BuildReport report)
    {
        var plistPath = $"{report.summary.outputPath}/Info.plist";
        var plist = new PlistDocument();
        plist.ReadFromFile(plistPath);
        plist.root.SetString("NSUserTrackingUsageDescription", "");
        plist.root.SetBoolean("UIFileSharingEnabled", true);
        plist.WriteToFile(plistPath);
    }
}
#endif
