using UnityEngine;
using Skyward.Aircraft;
using Skyward.CameraSystem;
using Skyward.UI;
using Skyward.Environment;
using Skyward.Persistence;

namespace Skyward.Core
{
    public sealed class SkywardSession : MonoBehaviour
    {
        [SerializeField] private AircraftConfig aircraftConfig;
        private bool paused;
        private SkywardSaveData profile;
        private AircraftController activeAircraft;

        public void SetAircraftConfig(AircraftConfig value) => aircraftConfig = value;

        private void Start()
        {
            profile = SkywardSettings.Data;
            AudioListener.volume = profile.masterVolume;
            if (FindObjectOfType<AircraftController>() != null) return;

            DemoAirportBuilder.Build();
            GameObject aircraft = ProceduralTrainerBuilder.Build();
            aircraft.transform.position = new Vector3(0f, 0.58f, -620f);
            aircraft.transform.rotation = Quaternion.identity;

            Rigidbody body = aircraft.AddComponent<Rigidbody>();
            body.mass = aircraftConfig != null ? aircraftConfig.massKg : 780f;
            body.centerOfMass = new Vector3(0f, -0.1f, -0.15f);

            AircraftConfig config = aircraftConfig;
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<AircraftConfig>();
                config.hideFlags = HideFlags.DontSave;
            }

            AircraftController controller = aircraft.AddComponent<AircraftController>();
            controller.Initialize(config);
            activeAircraft = controller;

            GameObject cameraObject = new GameObject("SKYWARD Flight Camera");
            cameraObject.tag = "MainCamera";
            FlightCameraRig cameraRig = cameraObject.AddComponent<FlightCameraRig>();
            cameraRig.SetTarget(aircraft.transform);

            GameObject hudObject = new GameObject("SKYWARD Flight HUD");
            FlightHud hud = hudObject.AddComponent<FlightHud>();
            hud.SetAircraft(controller);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                paused = !paused;
                Time.timeScale = paused ? 0f : 1f;
            }
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause && profile != null) SaveProfile();
        }

        private void OnApplicationQuit()
        {
            if (profile != null) SaveProfile();
            Time.timeScale = 1f;
        }

        private void SaveProfile()
        {
            if (activeAircraft != null)
                profile.totalFlightSeconds += Mathf.Max(0f, activeAircraft.FlightSeconds);
            SkywardSaveSystem.Save(profile);
        }

        private void OnDisable() => Time.timeScale = 1f;
    }
}