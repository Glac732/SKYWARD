using UnityEngine;
using Skyward.Aircraft;
using Skyward.CameraSystem;
using Skyward.UI;
using Skyward.Environment;
using Skyward.Persistence;
using Skyward.Audio;

namespace Skyward.Core
{
    public sealed class SkywardSession : MonoBehaviour
    {
        [SerializeField] private AircraftConfig aircraftConfig;
        private SkywardSaveData profile;
        private AircraftController activeAircraft;
        private float flightSecondsAlreadyRecorded;

        public void SetAircraftConfig(AircraftConfig value) => aircraftConfig = value;

        private void Start()
        {
            profile = SkywardSettings.Data;
            AudioListener.volume = profile.masterVolume;
            if (FindObjectOfType<AircraftController>() != null) return;

            int airportPreset = PlayerPrefs.GetInt("SKYWARD_ACTIVE_AIRPORT", 0);
            DemoAirportBuilder.Build(airportPreset);
            GameObject aircraft = ProceduralTrainerBuilder.Build();
            aircraft.transform.position = new Vector3(0f, 0.58f, -620f);
            aircraft.transform.rotation = Quaternion.identity;

            Rigidbody body = aircraft.AddComponent<Rigidbody>();
            body.mass = aircraftConfig != null ? aircraftConfig.massKg : 780f;
            body.centerOfMass = new Vector3(0f, -0.1f, -0.15f);
            BoxCollider fuselageCollider = aircraft.AddComponent<BoxCollider>();
            fuselageCollider.size = new Vector3(1.1f, 0.62f, 4.5f);
            fuselageCollider.center = new Vector3(0f, 0f, 0f);

            AircraftConfig config = aircraftConfig;
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<AircraftConfig>();
                config.hideFlags = HideFlags.DontSave;
            }

            AircraftController controller = aircraft.AddComponent<AircraftController>();
            controller.Initialize(config);
            activeAircraft = controller;

            SkywardEngineAudio engineAudio = aircraft.AddComponent<SkywardEngineAudio>();
            engineAudio.SetAircraft(controller);

            GameObject cameraObject = new GameObject("SKYWARD Flight Camera");
            cameraObject.tag = "MainCamera";
            FlightCameraRig cameraRig = cameraObject.AddComponent<FlightCameraRig>();
            cameraRig.SetTarget(aircraft.transform);

            GameObject hudObject = new GameObject("SKYWARD Flight HUD");
            FlightHud hud = hudObject.AddComponent<FlightHud>();
            hud.SetAircraft(controller);

            CockpitInstrumentPanel cockpit = aircraft.AddComponent<CockpitInstrumentPanel>();
            cockpit.SetAircraft(controller);
            SkywardGameMenu gameMenu = SkywardGameMenu.Create(controller);
            SkywardFlightResults.Create(controller, gameMenu);
            SkywardMobileControls.Create(controller);
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
            {
                float currentFlightSeconds = Mathf.Max(0f, activeAircraft.FlightSeconds);
                float unsavedSeconds = Mathf.Max(0f, currentFlightSeconds - flightSecondsAlreadyRecorded);
                profile.totalFlightSeconds += unsavedSeconds;
                flightSecondsAlreadyRecorded = currentFlightSeconds;
            }
            SkywardSaveSystem.Save(profile);
        }

        private void OnDisable() => Time.timeScale = 1f;
    }
}