using UnityEngine;

namespace Skyward.Environment
{
    /// <summary>Visual equipment that distinguishes the procedural aircraft presets.</summary>
    public static class AircraftVariantBuilder
    {
        public static void Apply(Transform aircraft, int preset)
        {
            Transform previous = aircraft.Find("Aircraft Variant Equipment");
            if (previous != null) Object.Destroy(previous.gameObject);
            if (preset == 0) return;

            GameObject rootObject = new GameObject("Aircraft Variant Equipment");
            rootObject.transform.SetParent(aircraft, false);
            Transform root = rootObject.transform;

            if (preset == 1)
            {
                // Touring trim: cabin windows, wing support struts and baggage pod.
                for (int side = -1; side <= 1; side += 2)
                {
                    Part("Tourer cabin window", PrimitiveType.Sphere,
                        new Vector3(side * 0.285f, 0.34f, -0.18f), new Vector3(0.035f, 0.22f, 0.48f),
                        new Color(0.08f, 0.24f, 0.32f), root);
                    Part("Tourer wing strut", PrimitiveType.Cylinder,
                        new Vector3(side * 2.1f, -0.05f, 0.0f), new Vector3(0.045f, 1.25f, 0.045f),
                        new Color(0.55f, 0.58f, 0.58f), root).transform.localRotation = Quaternion.Euler(0f, 0f, side * 22f);
                }
                Part("Tourer baggage pod", PrimitiveType.Capsule,
                    new Vector3(0f, -0.28f, -1.25f), new Vector3(0.28f, 0.24f, 1.05f),
                    new Color(0.72f, 0.75f, 0.76f), root).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            }
            else if (preset == 2)
            {
                // Aerobatic trim: wheel spats and colored wingtip smoke trails.
                Part("Aerobatic left wheel spat", PrimitiveType.Sphere,
                    new Vector3(-1.25f, -0.67f, -0.2f), new Vector3(0.34f, 0.22f, 0.56f),
                    new Color(0.78f, 0.12f, 0.08f), root);
                Part("Aerobatic right wheel spat", PrimitiveType.Sphere,
                    new Vector3(1.25f, -0.67f, -0.2f), new Vector3(0.34f, 0.22f, 0.56f),
                    new Color(0.78f, 0.12f, 0.08f), root);
                AddSmokeEmitter("Red wingtip smoke", new Vector3(-5.25f, 0f, -0.05f),
                    new Color(0.95f, 0.12f, 0.08f), root);
                AddSmokeEmitter("Blue wingtip smoke", new Vector3(5.25f, 0f, -0.05f),
                    new Color(0.10f, 0.38f, 0.95f), root);
            }
        }

        private static GameObject Part(string name, PrimitiveType type, Vector3 position,
            Vector3 scale, Color color, Transform parent)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            Collider collider = go.GetComponent<Collider>();
            if (collider != null) Object.Destroy(collider);
            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");
                renderer.sharedMaterial = new Material(shader) { color = color };
            }
            return go;
        }

        private static void AddSmokeEmitter(string name, Vector3 position, Color color, Transform parent)
        {
            GameObject emitterObject = new GameObject(name);
            emitterObject.transform.SetParent(parent, false);
            emitterObject.transform.localPosition = position;
            ParticleSystem particles = emitterObject.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.loop = true; main.playOnAwake = true; main.startLifetime = 2.2f;
            main.startSpeed = 2.5f; main.startSize = 0.42f; main.maxParticles = 180;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.startColor = color;
            var emission = particles.emission; emission.rateOverTime = 22f;
            var shape = particles.shape; shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 8f; shape.radius = 0.04f;
            var velocity = particles.velocityOverLifetime;
            velocity.enabled = true; velocity.z = new ParticleSystem.MinMaxCurve(-5f);
            var renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
        }
    }
}