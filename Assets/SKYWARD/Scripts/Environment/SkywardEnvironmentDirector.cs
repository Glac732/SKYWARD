using UnityEngine;
using Skyward.Aircraft;

namespace Skyward.Environment
{
    /// <summary>Runtime sky, cloud, visibility and precipitation profiles.</summary>
    public sealed class SkywardEnvironmentDirector : MonoBehaviour
    {
        private static readonly Color[] SkyTints =
        {
            new Color(0.72f, 0.82f, 0.94f),
            new Color(0.56f, 0.76f, 0.84f),
            new Color(0.72f, 0.78f, 0.86f)
        };
        private Transform cloudRoot;
        private ParticleSystem precipitation;
        private Light sun;
        private AudioSource ambience;
        private int weather;
        private AircraftController aircraft;

        public static SkywardEnvironmentDirector Create(Transform parent, int airport)
        {
            GameObject go = new GameObject("SKYWARD Dynamic Environment");
            go.transform.SetParent(parent, false);
            SkywardEnvironmentDirector director = go.AddComponent<SkywardEnvironmentDirector>();
            director.BuildClouds(airport);
            return director;
        }

        private void BuildClouds(int airport)
        {
            RenderSettings.skybox = RenderSettings.skybox;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = SkyTints[airport];
            RenderSettings.ambientEquatorColor = new Color(0.62f, 0.66f, 0.69f);
            RenderSettings.ambientGroundColor = new Color(0.24f, 0.25f, 0.23f);
            RenderSettings.fog = false;

            GameObject sunObject = new GameObject("SKYWARD Sun");
            sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.15f;
            sun.shadows = LightShadows.Soft;
            sunObject.transform.rotation = Quaternion.Euler(42f, -28f, 0f);

            cloudRoot = new GameObject("Procedural Cloud Layer").transform;
            cloudRoot.SetParent(transform, false);
            Material cloudMaterial = MakeMaterial(new Color(0.93f, 0.95f, 0.98f, 1f));
            Random.State previous = Random.state;
            Random.InitState(1800 + airport * 71);
            for (int i = 0; i < 36; i++)
            {
                GameObject cloud = new GameObject("Cloud Cluster " + i);
                cloud.transform.SetParent(cloudRoot, false);
                cloud.transform.localPosition = new Vector3(Random.Range(-900f, 900f), Random.Range(250f, 520f), Random.Range(-1000f, 1000f));
                cloud.transform.localScale = new Vector3(Random.Range(38f, 90f), Random.Range(8f, 20f), Random.Range(28f, 70f));
                for (int puff = 0; puff < 4; puff++)
                {
                    GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    sphere.name = "Cloud Puff";
                    sphere.transform.SetParent(cloud.transform, false);
                    sphere.transform.localPosition = new Vector3((puff - 1.5f) * 0.34f, Random.Range(-0.08f, 0.12f), Random.Range(-0.12f, 0.12f));
                    sphere.transform.localScale = new Vector3(0.65f, 0.65f, 0.72f);
                    Collider col = sphere.GetComponent<Collider>();
                    if (col != null) Destroy(col);
                    Renderer renderer = sphere.GetComponent<Renderer>();
                    renderer.sharedMaterial = cloudMaterial;
                }
            }
            Random.state = previous;

            GameObject rainObject = new GameObject("SKYWARD Rain");
            rainObject.transform.SetParent(transform, false);
            precipitation = rainObject.AddComponent<ParticleSystem>();
            var main = precipitation.main;
            main.loop = true; main.playOnAwake = false; main.startLifetime = 2.5f;
            main.startSpeed = 32f; main.startSize = 0.045f; main.maxParticles = 6000;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startColor = new Color(0.72f, 0.82f, 0.94f, 0.38f);
            var emission = precipitation.emission; emission.rateOverTime = 0f;
            var shape = precipitation.shape; shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(420f, 100f, 420f);
            var rendererRain = precipitation.GetComponent<ParticleSystemRenderer>();
            rendererRain.renderMode = ParticleSystemRenderMode.Stretch;
            rendererRain.lengthScale = 4f;
            rendererRain.material = MakeMaterial(new Color(0.65f, 0.78f, 0.9f, 0.5f));

            GameObject audioObject = new GameObject("SKYWARD Wind and Weather Ambience");
            audioObject.transform.SetParent(transform, false);
            ambience = audioObject.AddComponent<AudioSource>();
            ambience.clip = BuildWindLoop();
            ambience.loop = true;
            ambience.playOnAwake = false;
            ambience.spatialBlend = 0f;
            ambience.volume = 0f;
            ambience.Play();
            audioObject.AddComponent<AudioLowPassFilter>().cutoffFrequency = 1800f;
        }

        private AudioClip BuildWindLoop()
        {
            const int rate = 22050;
            const int length = rate * 3;
            float[] data = new float[length];
            float slow = 0f;
            for (int i = 0; i < length; i++)
            {
                slow = Mathf.Lerp(slow, Random.Range(-1f, 1f), 0.018f);
                float fade = Mathf.Min(1f, Mathf.Min(i / 256f, (length - i - 1) / 256f));
                data[i] = slow * fade * 0.55f;
            }
            AudioClip clip = AudioClip.Create("SKYWARD Wind Ambience", length, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public void ApplyWeather(int preset, AircraftController target)
        {
            weather = Mathf.Clamp(preset, 0, 4);
            aircraft = target;
            float wind = weather == 1 ? 8f : weather == 2 ? 12f : weather == 3 ? 5f : 0f;
            if (aircraft != null) aircraft.SetWind(wind, weather == 2 ? 90f : 240f);
            RenderSettings.fog = weather == 3 || weather == 4;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = weather == 3 ? 0.0018f : weather == 4 ? 0.0035f : 0.00025f;
            RenderSettings.ambientIntensity = weather == 4 ? 0.45f : weather == 3 ? 0.65f : 1f;
            if (sun != null) sun.intensity = weather == 4 ? 0.35f : weather == 3 ? 0.7f : 1.15f;
            if (precipitation != null)
            {
                var emission = precipitation.emission;
                emission.rateOverTime = weather == 4 ? 1100f : 0f;
                if (weather == 4 && !precipitation.isPlaying) precipitation.Play();
                if (weather != 4 && precipitation.isPlaying) precipitation.Stop();
            }
            if (cloudRoot != null)
                cloudRoot.gameObject.SetActive(weather != 0);
            if (ambience != null)
                ambience.volume = weather == 4 ? 0.22f : weather == 1 || weather == 2 ? 0.13f : weather == 3 ? 0.08f : 0.035f;
        }

        private void Update()
        {
            if (cloudRoot != null)
                cloudRoot.position += Vector3.right * (weather == 2 ? 2.2f : 0.7f) * Time.deltaTime;
            if (precipitation != null && aircraft != null)
                precipitation.transform.position = aircraft.transform.position + Vector3.up * 55f;
        }

        private static Material MakeMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            Material material = new Material(shader) { color = color };
            if (material.HasProperty("_Surface")) material.SetFloat("_Surface", 1f);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.EnableKeyword("_ALPHABLEND_ON");
            material.renderQueue = 3000;
            return material;
        }
    }
}