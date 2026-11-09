using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Skyward.Aircraft;
using Skyward.Core;
using Skyward.Environment;
using Skyward.UI;

namespace Skyward.Tests
{
    public class SKYWARDStartupSmokeTests
    {
        private GameObject sessionObject;
        private AircraftConfig config;

        [UnityTest]
        public IEnumerator SessionBootsAircraftAirportCameraHudAndCockpit()
        {
            config = ScriptableObject.CreateInstance<AircraftConfig>();
            sessionObject = new GameObject("SKYWARD startup smoke-test session");
            SkywardSession session = sessionObject.AddComponent<SkywardSession>();
            session.SetAircraftConfig(config);

            // Start() assembles the procedural airport, aircraft and UI on the next frame.
            yield return null;
            yield return null;

            AircraftController aircraft = Object.FindObjectOfType<AircraftController>();
            FlightHud hud = Object.FindObjectOfType<FlightHud>();
            CockpitInstrumentPanel cockpit = Object.FindObjectOfType<CockpitInstrumentPanel>();
            Camera mainCamera = Camera.main;
            SkywardEnvironmentDirector environment = Object.FindObjectOfType<SkywardEnvironmentDirector>();
            GameObject airport = environment != null ? environment.transform.parent.gameObject : null;

            Assert.IsNotNull(aircraft, "Session startup must create the trainer aircraft.");
            Assert.IsNotNull(aircraft.GetComponent<Rigidbody>(), "Trainer must have a Rigidbody.");
            Assert.AreEqual(3, aircraft.GetComponentsInChildren<WheelCollider>().Length,
                "Trainer must have three landing-gear WheelColliders.");
            Assert.IsNotNull(hud, "Session startup must create the flight HUD.");
            Assert.IsNotNull(cockpit, "Session startup must create cockpit instruments.");
            Assert.IsNotNull(mainCamera, "Session startup must create a tagged main camera.");
            Assert.IsNotNull(airport, "Session startup must create the selected airport environment.");
            Assert.IsNotNull(environment, "Airport must include the dynamic environment director.");
            Assert.IsNotNull(GraphicsSettings.defaultRenderPipeline,
                "The demo session must render through the configured Scriptable Render Pipeline.");
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Time.timeScale = 1f;
            SkywardEnvironmentDirector environment = Object.FindObjectOfType<SkywardEnvironmentDirector>();
            GameObject airport = environment != null ? environment.transform.parent.gameObject : null;
            AircraftController aircraft = Object.FindObjectOfType<AircraftController>();
            FlightHud hud = Object.FindObjectOfType<FlightHud>();
            CockpitInstrumentPanel cockpit = Object.FindObjectOfType<CockpitInstrumentPanel>();
            Camera camera = Camera.main;

            if (airport != null) Object.Destroy(airport);
            if (aircraft != null) Object.Destroy(aircraft.gameObject);
            if (hud != null) Object.Destroy(hud.gameObject);
            if (camera != null) Object.Destroy(camera.gameObject);
            if (sessionObject != null) Object.Destroy(sessionObject);
            if (config != null) Object.Destroy(config);
            yield return null;
        }
    }
}
