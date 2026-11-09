using System.Collections.Generic;
using UnityEngine;

namespace Skyward.Environment
{
    public static class DemoAirportBuilder
    {
        private static readonly Dictionary<Color, Material> MaterialCache = new Dictionary<Color, Material>();
        public static readonly string[] AirportNames = { "AL KHOR INTERNATIONAL", "COASTAL BAY AIRFIELD", "HIGHLAND REGIONAL" };
        public static GameObject Build() => Build(0);

        public static GameObject Build(int airportPreset)
        {
            airportPreset = Mathf.Clamp(airportPreset, 0, 2);
            Color terrainColor = airportPreset == 0 ? new Color(0.22f, 0.34f, 0.20f) :
                airportPreset == 1 ? new Color(0.20f, 0.34f, 0.31f) : new Color(0.30f, 0.34f, 0.30f);
            Color buildingColor = airportPreset == 0 ? new Color(0.42f, 0.46f, 0.49f) :
                airportPreset == 1 ? new Color(0.76f, 0.79f, 0.77f) : new Color(0.40f, 0.42f, 0.39f);
            GameObject root = new GameObject("SKYWARD Airport • " + AirportNames[airportPreset]);
            CreatePrimitive("Terrain", PrimitiveType.Cube, new Vector3(0f, -1.1f, 20f), new Vector3(1800f, 1f, 2200f), terrainColor, root.transform);
            CreatePrimitive("Runway", PrimitiveType.Cube, new Vector3(0f, -0.48f, 0f), new Vector3(32f, 0.12f, 1800f), new Color(0.12f, 0.13f, 0.14f), root.transform);
            CreatePrimitive("Runway centerline", PrimitiveType.Cube, new Vector3(0f, -0.405f, 0f), new Vector3(0.35f, 0.02f, 1500f), Color.white, root.transform);
            CreatePrimitive("Left runway edge", PrimitiveType.Cube, new Vector3(-15.2f, -0.405f, 0f), new Vector3(0.35f, 0.02f, 1780f), Color.white, root.transform);
            CreatePrimitive("Right runway edge", PrimitiveType.Cube, new Vector3(15.2f, -0.405f, 0f), new Vector3(0.35f, 0.02f, 1780f), Color.white, root.transform);

            for (int side = -1; side <= 1; side += 2)
            {
                float thresholdZ = side * 835f;
                for (int stripe = -5; stripe <= 5; stripe++)
                    CreatePrimitive("Threshold marking", PrimitiveType.Cube, new Vector3(stripe * 2.25f, -0.385f, thresholdZ), new Vector3(1.15f, 0.025f, 28f), Color.white, root.transform);
                for (int pair = -2; pair <= 2; pair++)
                    CreatePrimitive("Touchdown zone marking", PrimitiveType.Cube, new Vector3(pair * 5.5f, -0.385f, -side * 650f), new Vector3(1.0f, 0.025f, 36f), Color.white, root.transform);
            }

            CreatePrimitive("Taxiway", PrimitiveType.Cube, new Vector3(39f, -0.47f, -105f), new Vector3(14f, 0.10f, 310f), new Color(0.19f, 0.20f, 0.21f), root.transform);
            CreatePrimitive("Taxiway centre line", PrimitiveType.Cube, new Vector3(39f, -0.405f, -105f), new Vector3(0.22f, 0.02f, 290f), new Color(1f, 0.78f, 0.08f), root.transform);
            CreatePrimitive("Apron", PrimitiveType.Cube, new Vector3(75f, -0.49f, -110f), new Vector3(100f, 0.08f, 130f), new Color(0.27f, 0.28f, 0.29f), root.transform);

            for (int z = -840; z <= 840; z += 60)
            {
                CreateLight("Runway edge light L", new Vector3(-16.8f, -0.15f, z), root.transform);
                CreateLight("Runway edge light R", new Vector3(16.8f, -0.15f, z), root.transform);
            }
            for (int i = -4; i <= 4; i++)
            {
                CreateLight("Approach light", new Vector3(i * 5f, -0.15f, -900f), root.transform);
                CreateLight("Approach light", new Vector3(i * 5f, -0.15f, 900f), root.transform);
            }
            for (int z = -800; z <= 800; z += 100)
                CreatePrimitive("Centerline dash", PrimitiveType.Cube, new Vector3(0f, -0.39f, z), new Vector3(0.65f, 0.025f, 24f), Color.white, root.transform);

            CreatePrimitive("Terminal hangar", PrimitiveType.Cube, new Vector3(70f, 8f, -100f), new Vector3(70f, 16f, 55f), buildingColor, root.transform);
            CreatePrimitive("Control tower", PrimitiveType.Cube, new Vector3(-90f, 18f, -120f), new Vector3(12f, 36f, 12f), buildingColor, root.transform);
            CreatePrimitive("Tower cab glazing", PrimitiveType.Cube, new Vector3(-90f, 37f, -120f), new Vector3(18f, 4f, 18f), new Color(0.2f, 0.32f, 0.38f), root.transform);
            CreatePrimitive("Terminal annex", PrimitiveType.Cube, new Vector3(135f, 4f, -125f), new Vector3(45f, 8f, 38f), buildingColor, root.transform);
            if (airportPreset == 1)
            {
                for (int i = 0; i < 8; i++)
                    CreatePrimitive("Coastal water", PrimitiveType.Cube, new Vector3(440f + i * 90f, -0.62f, 300f), new Vector3(95f, 0.08f, 1800f), new Color(0.08f, 0.28f, 0.38f), root.transform);
                CreatePrimitive("Coastal promenade", PrimitiveType.Cube, new Vector3(420f, 0.2f, 0f), new Vector3(12f, 0.8f, 1700f), new Color(0.68f, 0.63f, 0.48f), root.transform);
            }
            else if (airportPreset == 2)
            {
                for (int i = 0; i < 22; i++)
                {
                    float x = (i % 2 == 0 ? -1f : 1f) * Random.Range(250f, 520f);
                    float z = Random.Range(-1000f, 1000f);
                    CreatePrimitive("Highland ridge", PrimitiveType.Sphere, new Vector3(x, Random.Range(60f, 140f), z),
                        new Vector3(Random.Range(90f, 170f), Random.Range(110f, 220f), Random.Range(100f, 240f)),
                        new Color(0.32f, 0.35f, 0.33f), root.transform);
                }
            }

            for (int i = 0; i < 14; i++)
            {
                float x = (i % 2 == 0 ? -1f : 1f) * Random.Range(120f, 210f);
                float z = Random.Range(-600f, 600f);
                CreatePrimitive("Distant hill", PrimitiveType.Sphere, new Vector3(x, Random.Range(12f, 35f), z),
                    new Vector3(Random.Range(45f, 90f), Random.Range(18f, 55f), Random.Range(60f, 130f)),
                    new Color(0.25f, 0.36f, 0.23f), root.transform);
            }
            SkywardEnvironmentDirector.Create(root.transform, airportPreset);
            return root;
        }

        private static GameObject CreateLight(string name, Vector3 position, Transform parent)
        {
            GameObject lightObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            lightObject.name = name;
            lightObject.transform.SetParent(parent);
            lightObject.transform.position = position;
            lightObject.transform.localScale = Vector3.one * 0.35f;
            Collider collider = lightObject.GetComponent<Collider>();
            if (collider != null) collider.enabled = false;
            Renderer renderer = lightObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = GetSharedMaterial(new Color(0.85f, 0.92f, 1f));
            }
            return lightObject;
        }

        private static GameObject CreatePrimitive(string name, PrimitiveType type, Vector3 position, Vector3 scale, Color color, Transform parent)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.localScale = scale;
            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = GetSharedMaterial(color);
            }
            return go;
        }

        private static Material GetSharedMaterial(Color color)
        {
            if (MaterialCache.TryGetValue(color, out Material cached) && cached != null)
                return cached;

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null)
            {
                Debug.LogError("SKYWARD: No supported Lit shader is available for airport materials.");
                return null;
            }

            Material material = new Material(shader) { color = color };
            MaterialCache[color] = material;
            return material;
        }
    }
}