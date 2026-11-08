using UnityEngine;

namespace Skyward.Environment
{
    public static class ProceduralTrainerBuilder
    {
        public static GameObject Build()
        {
            GameObject aircraft = new GameObject("SKYWARD Trainer Aircraft");
            CreatePart("Fuselage", PrimitiveType.Capsule, new Vector3(0f, 0f, 0f), new Vector3(0.55f, 0.62f, 4.5f), new Color(0.82f, 0.84f, 0.83f), aircraft.transform, new Vector3(90f, 0f, 0f));
            CreatePart("Left wing", PrimitiveType.Cube, new Vector3(-0.05f, 0f, 0f), new Vector3(10.9f, 0.12f, 1.45f), new Color(0.82f, 0.84f, 0.83f), aircraft.transform);
            CreatePart("Tailplane", PrimitiveType.Cube, new Vector3(0f, 0.18f, -3.75f), new Vector3(3.6f, 0.1f, 0.8f), new Color(0.82f, 0.84f, 0.83f), aircraft.transform);
            CreatePart("Fin", PrimitiveType.Cube, new Vector3(0f, 0.8f, -3.65f), new Vector3(0.12f, 1.3f, 0.9f), new Color(0.82f, 0.84f, 0.83f), aircraft.transform);
            CreatePart("Nose", PrimitiveType.Sphere, new Vector3(0f, 0f, 2.15f), new Vector3(0.52f, 0.52f, 0.65f), new Color(0.18f, 0.22f, 0.25f), aircraft.transform);
            CreatePart("Cockpit glazing", PrimitiveType.Sphere, new Vector3(0f, 0.42f, 0.55f), new Vector3(0.48f, 0.55f, 0.95f), new Color(0.12f, 0.25f, 0.32f), aircraft.transform);
            CreatePart("Left wheel", PrimitiveType.Cylinder, new Vector3(-1.25f, -0.68f, -0.2f), new Vector3(0.64f, 0.16f, 0.64f), Color.black, aircraft.transform, new Vector3(90f, 0f, 0f));
            CreatePart("Right wheel", PrimitiveType.Cylinder, new Vector3(1.25f, -0.68f, -0.2f), new Vector3(0.64f, 0.16f, 0.64f), Color.black, aircraft.transform, new Vector3(90f, 0f, 0f));
            CreatePart("Nose wheel", PrimitiveType.Cylinder, new Vector3(0f, -0.68f, 1.65f), new Vector3(0.44f, 0.14f, 0.44f), Color.black, aircraft.transform, new Vector3(90f, 0f, 0f));
            CreatePart("Propeller", PrimitiveType.Cube, new Vector3(0f, 0f, 2.55f), new Vector3(0.08f, 1.6f, 0.12f), new Color(0.12f, 0.12f, 0.12f), aircraft.transform);
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
                Material material = new Material(Shader.Find("Standard"));
                material.color = color;
                renderer.sharedMaterial = material;
            }
            return go;
        }
    }
}