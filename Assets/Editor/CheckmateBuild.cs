#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Checkmate
{
    public static class CheckmateBuild
    {
        public static void PrepareProject()
        {
            PlayerSettings.companyName = "Checkmate Project";
            PlayerSettings.productName = "Checkmate";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.checkmate.chess");
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.runInBackground = false;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel23;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.X86_64;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.stripEngineCode = true;
        }

        public static void EnsureScene()
        {
            const string folder = "Assets/Scenes";
            const string path = folder + "/ChessMatch.unity";
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var app = new GameObject("CheckmateApp");
            app.AddComponent<CheckmateBootstrap>();
            EditorSceneManager.SaveScene(scene, path);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(path, true) };
            AssetDatabase.Refresh();
        }

        [MenuItem("Checkmate/Build/Build Android APK")]
        public static void BuildAndroid()
        {
            CheckmateRuleSmokeTests.Run();
            PrepareProject();
            EnsureScene();
            const string output = "Builds/Android/Checkmate.apk";
            Directory.CreateDirectory("Builds/Android");
            var options = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/ChessMatch.unity" },
                locationPathName = output,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };
            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new System.Exception("Android build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);
            Debug.Log("Checkmate Android APK built: " + Path.GetFullPath(output));
        }
    }
}
#endif
