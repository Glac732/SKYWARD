using System.Collections.Generic;
using UnityEngine;

namespace Skyward.Environment
{
    public static class ProceduralTrainerBuilder
    {
        private static readonly Dictionary<Color, Material> MaterialCache = new Dictionary<Color, Material>();
        public static GameObject Build()
        {
            GameObject aircraft = new GameObject("SKYWARD Trainer Aircraft");
            CreatePart("Fuselage", PrimitiveType.Capsule, new Vector3(0f, 0f, 0f), new Vector3(0.55f, 0.62f, 4.5f), new Color(0.82f, 0.84f, 0.83f), aircraft.transform, new Vector3(90f, 0f, 0f));
            CreatePart("Left wing", PrimitiveType.Cube, new Vector3(-0.05f, 0f, 0f), new Vector3(10.9f, 0.12f, 1.45f), new Color(0.82f, 0.84f, 0.83f), aircraft.transform);
            CreatePart("Tailplane", PrimitiveType.Cube, new Vector3(0f, 0.18f, -3.75f), new Vector3(3.6f, 0.1f, 0.8f), new Color(0.82f, 0.84f, 0.83f), aircraft.transform);
            CreatePart("Fin", PrimitiveType.Cube, new Vector3(0f, 0.8f, -3.65f), new Vector3(0.12f, 1.3f, 0.9f), new Color(0.82f, 0.84f, 0.83f), aircraft.transform);
            CreatePart("Nose", PrimitiveType.Sphere, new Vector3(0f, 0f, 2.15f), new Vector3(0.52f, 0.52f, 0.65f), new Color(0.18f, 0.22f, 0.25f), aircraft.transform);
            CreatePart("Cockpit glazing", PrimitiveType.Sphere, new Vector3(0f, 0.42f, 0.55f), new Vector3(0.48f, 0.55f, 0.95f), new Color(0.12f, 0.25f, 0.32f), aircraft.transform);
            CreatePart("Left wheel", PrimitiveType.Cylinder, new Vector3(-1.25f, -0.68f, -0.2f), new Vector3(0.64f, 0.16f, 0.64f), Color.black, aircraft.transform, new Vector3(0f, 0f, 90f));
            CreatePart("Right wheel", PrimitiveType.Cylinder, new Vector3(1.25f, -0.68f, -0.2f), new Vector3(0.64f, 0.16f, 0.64f), Color.black, aircraft.transform, new Vector3(0f, 0f, 90f));
            CreatePart("Nose wheel", PrimitiveType.Cylinder, new Vector3(0f, -0.68f, 1.65f), new Vector3(0.44f, 0.14f, 0.44f), Color.black, aircraft.transform, new Vector3(0f, 0f, 90f));
            CreatePart("Propeller", PrimitiveType.Cube, new Vector3(0f, 0f, 2.55f), new Vector3(0.08f, 1.6f, 0.12f), new Color(0.12f, 0.12f, 0.12f), aircraft.transform);

            // Cabin interior: dashboard, seats, control column and instrument cluster.
            CreatePart("Instrument coaming", PrimitiveType.Cube, new Vector3(0f, 0.28f, 1.02f), new Vector3(1.02f, 0.20f, 0.34f), new Color(0.055f, 0.065f, 0.07f), aircraft.transform);
            CreatePart("Instrument panel", PrimitiveType.Cube, new Vector3(0f, 0.10f, 0.88f), new Vector3(0.94f, 0.38f, 0.10f), new Color(0.035f, 0.045f, 0.05f), aircraft.transform);
            CreatePart("Pilot seat", PrimitiveType.Cube, new Vector3(0f, -0.05f, -0.48f), new Vector3(0.62f, 0.16f, 0.66f), new Color(0.12f, 0.14f, 0.15f), aircraft.transform);
            CreatePart("Pilot seat back", PrimitiveType.Cube, new Vector3(0f, 0.22f, -0.78f), new Vector3(0.62f, 0.62f, 0.14f), new Color(0.12f, 0.14f, 0.15f), aircraft.transform);
            CreatePart("Control column", PrimitiveType.Cylinder, new Vector3(0f, -0.05f, 0.32f), new Vector3(0.07f, 0.42f, 0.07f), new Color(0.18f, 0.19f, 0.20f), aircraft.transform, new Vector3(0f, 0f, 0f));
            CreatePart("Control yoke", PrimitiveType.Cube, new Vector3(0f, 0.16f, 0.30f), new Vector3(0.46f, 0.055f, 0.055f), new Color(0.10f, 0.11f, 0.12f), aircraft.transform);
            for (int gauge = 0; gauge < 6; gauge++)
            {
                float x = (gauge % 3 - 1) * 0.27f;
                float y = gauge < 3 ? 0.18f : 0.02f;
                float z = 0.938f;
                CreatePart("Gauge bezel " + gauge, PrimitiveType.Cylinder, new Vector3(x, y, z), new Vector3(0.105f, 0.025f, 0.105f), new Color(0.02f, 0.025f, 0.03f), aircraft.transform, new Vector3(90f, 0f, 0f));
                CreatePart("Gauge face " + gauge, PrimitiveType.Cylinder, new Vector3(x, y, z + 0.018f), new Vector3(0.082f, 0.012f, 0.082f), new Color(0.78f, 0.82f, 0.80f), aircraft.transform, new Vector3(90f, 0f, 0f));
                CreatePart("Gauge needle " + gauge, PrimitiveType.Cube, new Vector3(x, y, z + 0.03f), new Vector3(0.009f, 0.006f, 0.055f), new Color(0.8f, 0.12f, 0.08f), aircraft.transform, new Vector3(0f, 0f, gauge * 23f - 40f));
            }
            CreatePart("Left wingtip", PrimitiveType.Sphere, new Vector3(-5.35f, 0f, 0f), new Vector3(0.22f, 0.10f, 0.26f), new Color(0.78f, 0.12f, 0.08f), aircraft.transform);
            CreatePart("Right wingtip", PrimitiveType.Sphere, new Vector3(5.35f, 0f, 0f), new Vector3(0.22f, 0.10f, 0.26f), new Color(0.08f, 0.22f, 0.78f), aircraft.transform);
            CreatePart("Left flap", PrimitiveType.Cube, new Vector3(-1.8f, -0.09f, -0.62f), new Vector3(3.0f, 0.045f, 0.34f), new Color(0.72f, 0.75f, 0.75f), aircraft.transform);
            CreatePart("Right flap", PrimitiveType.Cube, new Vector3(1.8f, -0.09f, -0.62f), new Vector3(3.0f, 0.045f, 0.34f), new Color(0.72f, 0.75f, 0.75f), aircraft.transform);
            return aircraft;
        }

        private static GameObject CreatePart(string name, PrimitiveType type, Vector3 localPosition, Vector3 localScale, Color color, Transform parent, Vector3 localEuler = default)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = localScale;
            go.transform.localEulerAngles = localEuler;
            Collider collider = go.GetComponent<Collider>();
            if (collider != null) collider.enabled = false;
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
                Debug.LogError("SKYWARD: No supported Lit shader is available for aircraft materials.");
                return null;
            }

            Material material = new Material(shader) { color = color };
            MaterialCache[color] = material;
            return material;
        }
    }
}