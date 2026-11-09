using UnityEngine;
using Skyward.Aircraft;

namespace Skyward.Audio
{
    /// <summary>Self-contained procedural piston-engine loop; no external audio asset required.</summary>
    [RequireComponent(typeof(AudioSource))]
    public sealed class SkywardEngineAudio : MonoBehaviour
    {
        private AircraftController aircraft;
        private AudioSource source;
        private float[] samples;
        private const int SampleRate = 22050;

        public void SetAircraft(AircraftController value) => aircraft = value;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0f;
            source.dopplerLevel = 0f;
            AudioClip clip = BuildEngineLoop();
            source.clip = clip;
            source.volume = 0.12f;
            source.Play();
        }

        private void Update()
        {
            if (aircraft == null || source == null) return;
            float throttle = aircraft.EngineRunning ? aircraft.Throttle : 0f;
            source.pitch = Mathf.Lerp(0.72f, 1.55f, throttle);
            source.volume = aircraft.EngineRunning ? Mathf.Lerp(0.035f, 0.19f, throttle) : 0f;
        }

        private AudioClip BuildEngineLoop()
        {
            const int seconds = 2;
            int count = SampleRate * seconds;
            samples = new float[count];
            // A harmonic-rich, softly filtered piston/propeller tone with a slow firing pulse.
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)SampleRate;
                float firing = Mathf.Sin(2f * Mathf.PI * 42f * t);
                float pulse = 0.72f + 0.28f * Mathf.Pow(Mathf.Max(0f, firing), 3f);
                float tone =
                    0.48f * Mathf.Sin(2f * Mathf.PI * 92f * t) +
                    0.24f * Mathf.Sin(2f * Mathf.PI * 184f * t + 0.3f) +
                    0.12f * Mathf.Sin(2f * Mathf.PI * 276f * t + 0.7f) +
                    0.06f * Mathf.Sin(2f * Mathf.PI * 460f * t);
                float fade = Mathf.Min(1f, Mathf.Min(i / 128f, (count - i - 1) / 128f));
                samples[i] = tone * pulse * fade * 0.42f;
            }
            AudioClip clip = AudioClip.Create("SKYWARD Procedural Piston Engine", count, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}