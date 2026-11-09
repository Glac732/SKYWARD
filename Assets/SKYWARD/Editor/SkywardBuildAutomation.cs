#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skyward.Editor
{
    /// <summary>
    /// Reproducible CI entry points. Each method creates the demo scene and
    /// builds a real player for the requested platform.
    /// </summary>
    public static class SkywardBuildAutomation
    {
        private const string ScenePath = "Assets/Scenes/SKYWARD_Demo.unity";

        public static void BuildWindows()
        {
            PrepareProject();
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono);
            PlayerSettings.defaultScreenWidth = 1600;
            PlayerSettings.defaultScreenHeight = 900;
            PlayerSettings.resizableWindow = true;

            string outputDirectory = "BuildOutput/Windows";
            Directory.CreateDirectory(outputDirectory);
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = GetEnabledScenes(),
                locationPathName = Path.Combine(outputDirectory, "SKYWARD.exe"),
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };
            ExecuteBuild(options, "Windows x64");
        }

        public static void BuildAndroid()
        {
            PrepareProject();
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.glac732.skyward");
            EditorUserBuildSettings.buildAppBundle = false;

            string outputDirectory = "BuildOutput/Android";
            Directory.CreateDirectory(outputDirectory);
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = GetEnabledScenes(),
                locationPathName = Path.Combine(outputDirectory, "SKYWARD.apk"),
                target = BuildTarget.Android,
                options = BuildOptions.None
            };
            ExecuteBuild(options, "Android ARM64 APK");
        }

        private static void PrepareProject()
        {
            Debug.Log("SKYWARD CI: preparing URP, trainer configuration and demo scene.");
            SkywardSceneSetup.CreateDemoScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (!File.Exists(ScenePath))
                throw new InvalidOperationException("SKYWARD CI: demo scene was not created at " + ScenePath);

            if (GraphicsSettings.defaultRenderPipeline == null)
                throw new InvalidOperationException("SKYWARD CI: Universal Render Pipeline asset was not assigned.");

            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            bool sceneEnabled = false;
            foreach (EditorBuildSettingsScene scene in scenes)
                if (scene.path == ScenePath && scene.enabled) sceneEnabled = true;

            if (!sceneEnabled)
                throw new InvalidOperationException("SKYWARD CI: demo scene is not enabled in Build Settings.");

            PlayerSettings.companyName = "SKYWARD Studio";
            PlayerSettings.productName = "SKYWARD";
            PlayerSettings.runInBackground = true;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Standalone, "com.glac732.skyward");
            Debug.Log("SKYWARD CI: project preparation checks passed.");
        }

        private static string[] GetEnabledScenes()
        {
            string[] scenes = Array.ConvertAll(
                Array.FindAll(EditorBuildSettings.scenes, scene => scene.enabled),
                scene => scene.path);

            if (scenes.Length == 0)
                throw new InvalidOperationException("SKYWARD CI: no enabled scenes are configured.");

            return scenes;
        }

        private static void ExecuteBuild(BuildPlayerOptions options, string label)
        {
            Debug.Log("SKYWARD CI: starting " + label + " build.");
            BuildReport report = BuildPipeline.BuildPlayer(options);
            Debug.Log("SKYWARD CI: " + label + " result = " + report.summary.result +
                      "; errors = " + report.summary.totalErrors +
                      "; warnings = " + report.summary.totalWarnings +
                      "; size = " + report.summary.totalSize + " bytes.");

            if (report.summary.result != BuildResult.Succeeded || report.summary.totalErrors > 0)
                throw new InvalidOperationException("SKYWARD CI: " + label + " build failed. Inspect the Unity Editor log and Build Report.");

            Debug.Log("SKYWARD CI: " + label + " build completed successfully.");
        }
    }
}
#endif
