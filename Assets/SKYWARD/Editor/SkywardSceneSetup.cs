#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Skyward.Core;
using Skyward.Aircraft;

namespace Skyward.Editor
{
    public static class SkywardSceneSetup
    {
        [MenuItem("SKYWARD/Create / Rebuild Demo Scene")]
        public static void CreateDemoScene()
        {
            EnsureUniversalRenderPipeline();
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");

            AircraftConfig config = AssetDatabase.LoadAssetAtPath<AircraftConfig>("Assets/SKYWARD/TrainerConfig.asset");
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<AircraftConfig>();
                AssetDatabase.CreateAsset(config, "Assets/SKYWARD/TrainerConfig.asset");
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject sessionObject = new GameObject("SKYWARD Session");
            SkywardSession session = sessionObject.AddComponent<SkywardSession>();
            session.SetAircraftConfig(config);

            GameObject light = new GameObject("Directional Light");
            Light directional = light.AddComponent<Light>();
            directional.type = LightType.Directional;
            directional.intensity = 1.15f;
            light.transform.rotation = Quaternion.Euler(45f, -30f, 0f);

            const string scenePath = "Assets/Scenes/SKYWARD_Demo.unity";
            EditorSceneManager.SaveScene(scene, scenePath);

            // Ensure the generated demo scene is included when a standalone player is built.
            var buildScenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            bool alreadyIncluded = false;
            for (int i = 0; i < buildScenes.Count; i++)
            {
                if (buildScenes[i].path != scenePath) continue;
                buildScenes[i] = new EditorBuildSettingsScene(scenePath, true);
                alreadyIncluded = true;
                break;
            }
            if (!alreadyIncluded)
                buildScenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = buildScenes.ToArray();

            AssetDatabase.SaveAssets();
            Debug.Log("SKYWARD demo scene created. Open Assets/Scenes/SKYWARD_Demo.unity and press Play.");
        }

        private static void EnsureUniversalRenderPipeline()
        {
            const string settingsFolder = "Assets/SKYWARD/Settings";
            const string rendererPath = settingsFolder + "/SKYWARD_Renderer.asset";
            const string pipelinePath = settingsFolder + "/SKYWARD_URP.asset";

            if (!AssetDatabase.IsValidFolder(settingsFolder))
                AssetDatabase.CreateFolder("Assets/SKYWARD", "Settings");

            UniversalRendererData rendererData = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(rendererPath);
            if (rendererData == null)
            {
                rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
                rendererData.name = "SKYWARD Universal Renderer";
                AssetDatabase.CreateAsset(rendererData, rendererPath);
            }

            UniversalRenderPipelineAsset pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(pipelinePath);
            if (pipeline == null)
            {
                pipeline = UniversalRenderPipelineAsset.Create(rendererData);
                if (pipeline == null)
                    throw new System.InvalidOperationException("SKYWARD: Unity could not create the URP pipeline asset.");
                pipeline.name = "SKYWARD URP";
                pipeline.supportsHDR = true;
                pipeline.renderScale = 1f;
                pipeline.msaaSampleCount = 2;
                pipeline.shadowDistance = 120f;
                AssetDatabase.CreateAsset(pipeline, pipelinePath);
            }

            GraphicsSettings.defaultRenderPipeline = pipeline;
            QualitySettings.renderPipeline = null;
            EditorUtility.SetDirty(rendererData);
            EditorUtility.SetDirty(pipeline);
            AssetDatabase.SaveAssets();
        }
    }
}
#endif