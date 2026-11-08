#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Skyward.Core;
using Skyward.Aircraft;

namespace Skyward.Editor
{
    public static class SkywardSceneSetup
    {
        [MenuItem("SKYWARD/Create / Rebuild Demo Scene")]
        public static void CreateDemoScene()
        {
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

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/SKYWARD_Demo.unity");
            AssetDatabase.SaveAssets();
            Debug.Log("SKYWARD demo scene created. Open Assets/Scenes/SKYWARD_Demo.unity and press Play.");
        }
    }
}
#endif