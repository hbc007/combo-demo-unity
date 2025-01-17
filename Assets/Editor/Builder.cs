using UnityEditor;
using System;
using UnityEngine;
using System.IO;
using System.Diagnostics;

public class Builder : EditorWindow
{
    internal static class GUIProps
    {
        public enum Platform
        {
            Android,
            iOS,
            Windows,
            Linux,
            Mac,
        }

        public static Platform selectedPlatform;

        public static Platform currentPlatform
        {
            get
            {
                if (Application.platform == RuntimePlatform.WindowsPlayer
                || Application.platform == RuntimePlatform.WindowsEditor)
                    return Platform.Windows;
                else if (Application.platform == RuntimePlatform.OSXPlayer
                || Application.platform == RuntimePlatform.OSXEditor)
                    return Platform.Mac;
                else
                    return Platform.Linux;
            }
        }
    }

    internal static class GlobalProps
    {
        public static string gameId = "demo";
        public static string publishableKey = "pk_Kk7tOa5keufEdZCmfoOel2ffe9WS6fPV";
        public static string endpoint = "https://api.dev.seayoo.com";
        public static string demoEndpoint = "https://combo-demo.dev.seayoo.com";

        public static string exportPath = "outputs/android";
        public static string bundleVersion = "1.0.0";
    }

    [MenuItem("ComboSDK/Build Demo", false, 9)]
    static void BuildDemo()
    {
        GetWindow<Builder>("Build Demo");
    }

    [MenuItem("ComboSDK/Archive", false, 1)]
    static void Archive()
    {
        AssetDatabase.Refresh();
        var rootDir = GetRootDir();
        var archiveDir = Path.Combine(rootDir, "Archive", "com.seayoo.sdk");
        if (Directory.Exists(archiveDir))
        {
            Directory.Delete(archiveDir, true);
        }

        Directory.CreateDirectory(archiveDir);
        var sourceDirName = Path.Combine(rootDir, "Packages/com.seayoo.sdk");
        DirectoryCopy(sourceDirName, archiveDir);
        CopyFile(Path.Combine(rootDir, "CHANGELOG.md"), Path.Combine(archiveDir, "CHANGELOG.md"));

        UnityEngine.Debug.Log("Finished to Archive => " + sourceDirName);
    }

    private static void BuildProject()
    {
        Process process = new Process();

        process.StartInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        process.Start();

        process.StandardInput.WriteLine("call build.bat");

        process.StandardInput.Close();

        // 读取命令的输出结果
        string output = process.StandardOutput.ReadToEnd();

        UnityEngine.Debug.LogWarning(output);

        // 关闭进程
        process.Close();
    }

    static void BuildWindowsDemo()
    {
        var exportPath = Environment.GetEnvironmentVariable("EXPORT_PATH");
        PlayerSettings.bundleVersion = Environment.GetEnvironmentVariable("BUNDLE_VERSION");
        CreateDir(exportPath);
        Build(
            new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/Login.unity", "Assets/Scenes/Game.unity" },
                locationPathName = Path.Combine(exportPath, "combosdk-unity-demo.exe"),
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            }
        );
    }

    static void BuildAndroidDemo()
    {
        var exportPath = Environment.GetEnvironmentVariable("EXPORT_PATH");
        PlayerSettings.bundleVersion = Environment.GetEnvironmentVariable("BUNDLE_VERSION");
        var splitVer = PlayerSettings.bundleVersion.Split(new char[] { '.' }, 2);
        if (splitVer.Length > 1)
            PlayerSettings.Android.bundleVersionCode = int.Parse(splitVer[0]);
        CreateDir(exportPath);

        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        EditorUserBuildSettings.exportAsGoogleAndroidProject = true;

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] { "Assets/Scenes/Login.unity", "Assets/Scenes/Game.unity" };
        buildPlayerOptions.locationPathName = exportPath; // 设置输出路径
        buildPlayerOptions.target = BuildTarget.Android;
#if UNITY_2019_1_OR_NEWER
        buildPlayerOptions.options = BuildOptions.AllowDebugging;
#else
        buildPlayerOptions.options = BuildOptions.AcceptExternalModificationsToPlayer; // 这个选项会导出Android工程，而不是构建APK
#endif

        Build(buildPlayerOptions);
    }

    static void BuildIOSDemo()
    {
        var exportPath = Environment.GetEnvironmentVariable("EXPORT_PATH");
        PlayerSettings.bundleVersion = Environment.GetEnvironmentVariable("BUNDLE_VERSION");
        CreateDir(exportPath);
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] { "Assets/Scenes/Login.unity", "Assets/Scenes/Game.unity" };
        buildPlayerOptions.locationPathName = exportPath; // 设置输出路径
        buildPlayerOptions.target = BuildTarget.iOS;
#if UNITY_2019_1_OR_NEWER
        buildPlayerOptions.options = BuildOptions.AllowDebugging;
#else
        buildPlayerOptions.options = BuildOptions.AcceptExternalModificationsToPlayer; // 这个选项会导出Android工程，而不是构建APK
#endif
        Build(buildPlayerOptions);
    }

    private static void Build(BuildPlayerOptions buildPlayerOptions)
    {
        BuildParams.Save();
        AssetDatabase.Refresh();
        BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildParams.Clear();
        AssetDatabase.Refresh();
    }

    static bool CreateDir(string path)
    {
        try
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return true;
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("Create directory failed: " + e.Message);
        }
        return false;
    }

    public static bool CopyFile(string sourcePath, string targetPath)
    {
        try
        {
            File.Copy(sourcePath, targetPath, true);
            UnityEngine.Debug.Log("Copy \"" + sourcePath + "\" to \"" + targetPath + "\" succeed");
            return true;
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("Copy failed：" + e.Message);
        }
        return false;
    }

    // Used for Jenkins
    public static void UpdateComboSDKSettings()
    {
        var gameId = Environment.GetEnvironmentVariable("COMBOSDK_GAME_ID");
        var publishableKey = Environment.GetEnvironmentVariable("COMBOSDK_PUBLISHABLE_KEY");
        var endpoint = Environment.GetEnvironmentVariable("COMBOSDK_ENDPOINT");

        var scriptableObject = AssetDatabase.LoadAssetAtPath<Combo.Configuration>("Assets/ComboSDK/Resources/ComboSDKSettings.asset");

        scriptableObject.GameId = gameId;
        scriptableObject.PublishableKey = publishableKey;
        scriptableObject.Endpoint = endpoint;

        // Mark the ScriptableObject as dirty so Unity knows it needs to save changes
        EditorUtility.SetDirty(scriptableObject);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        UnityEngine.Debug.Log($"gameId = {gameId}, publishableKey = {publishableKey}, endpoint = {endpoint}");
    }

    public static void DirectoryCopy(
        string sourceDirName,
        string destDirName,
        bool copySubDirs = true
    )
    {
        DirectoryInfo dir = new DirectoryInfo(sourceDirName);
        DirectoryInfo[] dirs = dir.GetDirectories();

        // If the source directory does not exist, throw an exception.
        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException(
                "Source directory does not exist or could not be found: " + sourceDirName
            );
        }

        // If the destination directory does not exist, create it.
        if (!Directory.Exists(destDirName))
        {
            Directory.CreateDirectory(destDirName);
        }

        // Get the file contents of the directory to copy.
        FileInfo[] files = dir.GetFiles();

        foreach (FileInfo file in files)
        {
            // Create the path to the new copy of the file.
            string temppath = Path.Combine(destDirName, file.Name);

            // Copy the file.
            file.CopyTo(temppath, true);
        }

        // If copySubDirs is true, copy the subdirectories.
        if (copySubDirs)
        {
            foreach (DirectoryInfo subdir in dirs)
            {
                // Create the subdirectory.
                string temppath = Path.Combine(destDirName, subdir.Name);

                // Copy the subdirectories.
                DirectoryCopy(subdir.FullName, temppath, copySubDirs);
            }
        }
    }

    private static string GetRootDir()
    {
        string projectRootPath = Path.GetFullPath(Application.dataPath);
        return Path.GetDirectoryName(projectRootPath);
    }

    private void OnGUI()
    {
        GUIStyle largeFontBoldStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 13 };

        GUILayout.Label("General Build Parameters", largeFontBoldStyle);

        GlobalProps.gameId = EditorGUILayout.TextField("GameId", GlobalProps.gameId);
        GlobalProps.publishableKey = EditorGUILayout.TextField(
            "PublishableKey",
            GlobalProps.publishableKey
        );
        GlobalProps.endpoint = EditorGUILayout.TextField("Endpoint", GlobalProps.endpoint);
        GlobalProps.demoEndpoint = EditorGUILayout.TextField(
            "DemoEndpoint",
            GlobalProps.demoEndpoint
        );
        GlobalProps.bundleVersion = EditorGUILayout.TextField(
            "BundleVersion",
            GlobalProps.bundleVersion
        );

        GUILayout.Label("Start Build", largeFontBoldStyle);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("TargetPlatform", GUILayout.Width(147));
        var newSelectedPlatform = (GUIProps.Platform)
            GUILayout.Toolbar(
                (int)GUIProps.selectedPlatform,
                new string[] { "Android", "iOS", "Windows" }
            );
        if (GUIProps.selectedPlatform != newSelectedPlatform)
        {
            GUIProps.selectedPlatform = newSelectedPlatform;
            GlobalProps.exportPath = $"outputs/{GUIProps.selectedPlatform}".ToLower();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GlobalProps.exportPath = EditorGUILayout.TextField("ExportPath", GlobalProps.exportPath);
        if (GUILayout.Button("Choose", GUILayout.Width(60)))
        {
            GlobalProps.exportPath = EditorUtility.OpenFolderPanel("ExportPath", "", "");
            Repaint();
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Start Build"))
        {
            Environment.SetEnvironmentVariable("COMBOSDK_GAME_ID", GlobalProps.gameId);
            Environment.SetEnvironmentVariable(
                "COMBOSDK_PUBLISHABLE_KEY",
                GlobalProps.publishableKey
            );
            Environment.SetEnvironmentVariable("COMBOSDK_ENDPOINT", GlobalProps.endpoint);
            Environment.SetEnvironmentVariable("DEMO_ENDPOINT", GlobalProps.demoEndpoint);
            Environment.SetEnvironmentVariable("BUNDLE_VERSION", GlobalProps.bundleVersion);

            Environment.SetEnvironmentVariable("EXPORT_PATH", GlobalProps.exportPath);
            UpdateComboSDKSettings();

            switch (GUIProps.selectedPlatform)
            {
                case GUIProps.Platform.Android:
                    {
                        BuildAndroidDemo();
                        break;
                    }
                case GUIProps.Platform.iOS:
                    {
                        BuildIOSDemo();
                        break;
                    }
                case GUIProps.Platform.Windows:
                    {
                        BuildWindowsDemo();
                        break;
                    }
            }
        }

        if (GUILayout.Button($"Show In {(GUIProps.currentPlatform == GUIProps.Platform.Windows ? "Explorer" : "Finder")}"))
        {
            if (GUIProps.currentPlatform == GUIProps.Platform.Windows)
            {
                Process.Start(
                    "explorer.exe",
                    "/select," + GlobalProps.exportPath.Replace('/', '\\')
                );
            }
            else if (GUIProps.currentPlatform == GUIProps.Platform.Mac)
            {
                Process.Start("open", GlobalProps.exportPath);
            }
            else if (GUIProps.currentPlatform == GUIProps.Platform.Linux)
            {
                Process.Start("xdg-open", GlobalProps.exportPath);
            }
        }
    }
}
