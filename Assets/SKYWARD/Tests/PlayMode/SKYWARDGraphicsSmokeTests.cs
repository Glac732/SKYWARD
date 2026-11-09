using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Skyward.Aircraft;
using Skyward.Core;

namespace Skyward.Tests
{
    public class SKYWARDGraphicsSmokeTests
    {
        private GameObject sessionObject;
        private AircraftConfig config;
        private RenderTexture renderTexture;
        private Texture2D capturedFrame;

        [UnityTest]
        public IEnumerator SceneRendersAndCapturesFrameWithFrameTimingDiagnostics()
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
                Assert.Ignore("This runner has no graphics device; visual rendering requires a graphics-enabled runner.");

            config = ScriptableObject.CreateInstance<AircraftConfig>();
            sessionObject = new GameObject("SKYWARD graphics smoke-test session");
            SkywardSession session = sessionObject.AddComponent<SkywardSession>();
            session.SetAircraftConfig(config);
            yield return null;
            yield return null;

            Camera camera = Camera.main;
            Assert.IsNotNull(camera, "A MainCamera must exist before graphics capture.");
            Assert.IsNotNull(GraphicsSettings.defaultRenderPipeline, "URP must be assigned before rendering.");

            float frameTimeSum = 0f;
            float slowestFrame = 0f;
            const int sampleFrames = 120;
            for (int i = 0; i < sampleFrames; i++)
            {
                yield return null;
                float frameTime = Time.unscaledDeltaTime;
                if (frameTime > 0f && !float.IsNaN(frameTime) && !float.IsInfinity(frameTime))
                {
                    frameTimeSum += frameTime;
                    slowestFrame = Mathf.Max(slowestFrame, frameTime);
                }
            }

            float averageFrameTimeMs = frameTimeSum / sampleFrames * 1000f;
            float averageFps = frameTimeSum > 0f ? sampleFrames / frameTimeSum : 0f;
            Debug.Log($"SKYWARD GRAPHICS DIAGNOSTIC: samples={sampleFrames}, averageFPS={averageFps:0.0}, averageFrameMs={averageFrameTimeMs:0.00}, slowestFrameMs={slowestFrame * 1000f:0.00}, graphicsDevice={SystemInfo.graphicsDeviceType}, resolution={Screen.width}x{Screen.height}");

            renderTexture = new RenderTexture(960, 540, 24, RenderTextureFormat.ARGB32);
            renderTexture.Create();
            RenderTexture previousTarget = camera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;
            camera.targetTexture = renderTexture;
            camera.Render();
            RenderTexture.active = renderTexture;

            capturedFrame = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
            capturedFrame.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            capturedFrame.Apply();
            camera.targetTexture = previousTarget;
            RenderTexture.active = previousActive;

            Color32[] pixels = capturedFrame.GetPixels32();
            Color32 firstPixel = pixels[0];
            int visiblyDifferentPixels = 0;
            for (int i = 1; i < pixels.Length; i++)
            {
                Color32 pixel = pixels[i];
                if (Mathf.Abs(pixel.r - firstPixel.r) > 8 ||
                    Mathf.Abs(pixel.g - firstPixel.g) > 8 ||
                    Mathf.Abs(pixel.b - firstPixel.b) > 8)
                    visiblyDifferentPixels++;
            }

            Assert.Greater(visiblyDifferentPixels, 100,
                "Rendered frame appears uniform/blank; inspect URP, camera and scene setup.");

            string outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "artifacts", "graphics");
            Directory.CreateDirectory(outputDirectory);
            string screenshotPath = Path.Combine(outputDirectory, "SKYWARD-960x540-smoke.png");
            File.WriteAllBytes(screenshotPath, capturedFrame.EncodeToPNG());
            Debug.Log("SKYWARD GRAPHICS DIAGNOSTIC: saved rendered frame to " + screenshotPath);
            Assert.IsTrue(File.Exists(screenshotPath), "Graphics smoke screenshot must be saved as a test artifact.");
            Assert.Greater(averageFps, 0f, "Frame timing must produce a valid positive measurement.");
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Time.timeScale = 1f;
            if (renderTexture != null)
            {
                renderTexture.Release();
                Object.Destroy(renderTexture);
            }
            if (capturedFrame != null) Object.Destroy(capturedFrame);

            GameObject airport = GameObject.Find("SKYWARD Demo Airport");
            AircraftController aircraft = Object.FindObjectOfType<AircraftController>();
            Camera camera = Camera.main;
            if (airport != null) Object.Destroy(airport);
            if (aircraft != null) Object.Destroy(aircraft.gameObject);
            if (camera != null) Object.Destroy(camera.gameObject);
            if (sessionObject != null) Object.Destroy(sessionObject);
            if (config != null) Object.Destroy(config);
            yield return null;
        }
    }
}
