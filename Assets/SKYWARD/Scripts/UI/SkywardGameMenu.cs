using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Skyward.Aircraft;
using Skyward.Core;
using Skyward.CameraSystem;
using Skyward.Environment;

namespace Skyward.UI
{
    /// <summary>Playable front-end, aircraft presets, flight school, mission board and live settings.</summary>
    public sealed class SkywardGameMenu : MonoBehaviour
    {
        private AircraftController aircraft;
        private Canvas canvas;
        private GameObject panel;
        private Text title;
        private Text body;
        private Text status;
        private int aircraftIndex;
        private int missionIndex;
        private int lessonIndex;
        private bool menuOpen = true;
        private float wind;
        private int weatherIndex;
        private bool flightRecorded;
        private int airportIndex;
        private SkywardEnvironmentDirector environment;
        private static readonly string[] AirportLabels = { "AL KHOR • DESERT", "COASTAL BAY • SHORELINE", "HIGHLAND • MOUNTAINS" };
        private static readonly string[] AircraftNames = { "SKYWARD 152 • TRAINER", "SKYWARD TOURER • TOURING", "SKYWARD AERO • AEROBATIC" };
        private static readonly string[] MissionNames = { "FIRST FLIGHT", "CIRCUIT & LANDING", "CROSS-COUNTRY", "HIGH ALTITUDE", "SOFT TOUCHDOWN", "WINDY APPROACH" };
        private static readonly string[] LessonNames = { "01  PREFLIGHT & CONTROLS", "02  TAKE-OFF", "03  CLIMB & LEVEL FLIGHT", "04  APPROACH & LANDING" };
        private static readonly string[] WeatherNames = { "CLEAR • CALM", "BREEZY • 8 m/s", "CROSSWIND • 12 m/s", "DUSK • FOG", "RAIN • LOW VISIBILITY" };

        public static void Create(AircraftController target)
        {
            GameObject root = new GameObject("SKYWARD Front End");
            SkywardGameMenu menu = root.AddComponent<SkywardGameMenu>();
            menu.aircraft = target;
        }

        private void Start()
        {
            environment = FindObjectOfType<SkywardEnvironmentDirector>();
            airportIndex = Mathf.Clamp(PlayerPrefs.GetInt("SKYWARD_ACTIVE_AIRPORT", 0), 0, 2);
            weatherIndex = Mathf.Clamp(PlayerPrefs.GetInt("SKYWARD_WEATHER", 0), 0, 4);
            aircraftIndex = Mathf.Clamp(PlayerPrefs.GetInt("SKYWARD_AIRCRAFT", 0), 0, 2);
            BuildCanvas();
            if (aircraftIndex != 0) ApplyAircraft();
            ApplyWeather();
            ShowHome();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (menuOpen) CloseMenu();
                else ShowHome();
            }
            if (aircraft == null) aircraft = FindObjectOfType<AircraftController>();
            if (status != null && aircraft != null)
            {
                bool lessonPassedThisFrame = false;
                int lesson = PlayerPrefs.GetInt("SKYWARD_ACTIVE_LESSON", -1);
                bool lessonComplete = lesson == 0
                    ? aircraft.EngineRunning && !aircraft.ParkingBrake
                    : lesson == 1
                        ? aircraft.HasTakenOff
                        : lesson == 2
                            ? aircraft.MaximumAltitude >= 300f && aircraft.FlightSeconds >= 30f
                            : lesson == 3 && aircraft.HasLanded && aircraft.TouchdownOnRunway;
                if (lesson >= 0 && lessonComplete && PlayerPrefs.GetInt("SKYWARD_LESSON_DONE_" + lesson, 0) == 0)
                {
                    PlayerPrefs.SetInt("SKYWARD_LESSON_DONE_" + lesson, 1);
                    PlayerPrefs.Save();
                    status.text = "FLIGHT SCHOOL PASSED  •  " + LessonNames[lesson];
                    lessonPassedThisFrame = true;
                }
                if (aircraft.HasLanded && !flightRecorded) RecordFlight();
                int mission = PlayerPrefs.GetInt("SKYWARD_ACTIVE_MISSION", 0);
                bool complete = mission == 0
                    ? aircraft.HasTakenOff && aircraft.HasLanded && aircraft.TouchdownOnRunway
                    : mission == 1
                        ? aircraft.FlightSeconds >= 90f && aircraft.HasTakenOff && aircraft.HasLanded && aircraft.TouchdownOnRunway
                        : mission == 2
                            ? aircraft.MaximumAltitude >= 800f && aircraft.FlightSeconds >= 180f
                            : mission == 3
                                ? aircraft.MaximumAltitude >= 1500f
                                : mission == 4
                                    ? aircraft.HasLanded && aircraft.TouchdownOnRunway && aircraft.TouchdownVerticalSpeed > -2.0f
                                    : aircraft.HasTakenOff && aircraft.HasLanded && aircraft.TouchdownOnRunway &&
                                      aircraft.TouchdownVerticalSpeed > -3.0f && PlayerPrefs.GetInt("SKYWARD_WEATHER", 0) == 2;
                if (complete && PlayerPrefs.GetInt("SKYWARD_MISSION_DONE_" + mission, 0) == 0)
                {
                    PlayerPrefs.SetInt("SKYWARD_MISSION_DONE_" + mission, 1);
                    PlayerPrefs.Save();
                    status.text = "MISSION COMPLETE  •  " + MissionNames[mission];
                }
                else if (!lessonPassedThisFrame)
                    status.text = $"IAS {aircraft.AirspeedKnots:000} kt   ALT {aircraft.AltitudeMeters:0000} m   {aircraft.FlightPhase}";
            }
        }

        private void BuildCanvas()
        {
            if (FindObjectOfType<EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("SKYWARD Event System", typeof(EventSystem), typeof(StandaloneInputModule));
                eventSystem.transform.SetParent(transform, false);
            }
            GameObject canvasObject = new GameObject("Front End Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            panel = new GameObject("Main panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvasObject.transform, false);
            RectTransform p = panel.GetComponent<RectTransform>();
            p.anchorMin = new Vector2(0.5f, 0.5f); p.anchorMax = new Vector2(0.5f, 0.5f);
            p.pivot = new Vector2(0.5f, 0.5f); p.sizeDelta = new Vector2(760, 820);
            panel.GetComponent<Image>().color = new Color(0.015f, 0.035f, 0.055f, 0.96f);

            title = MakeText("SKYWARD", new Vector2(0, 275), new Vector2(700, 65), 42, TextAnchor.MiddleCenter, panel.transform);
            title.fontStyle = FontStyle.Bold;
            body = MakeText("", new Vector2(0, 75), new Vector2(650, 300), 23, TextAnchor.MiddleCenter, panel.transform);
            body.lineSpacing = 1.25f;
            status = MakeText("", new Vector2(0, -510), new Vector2(1000, 35), 17, TextAnchor.MiddleCenter, canvasObject.transform);
        }

        private Text MakeText(string value, Vector2 position, Vector2 size, int fontSize, TextAnchor anchor, Transform parent)
        {
            GameObject go = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position; rect.sizeDelta = size;
            Text text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize; text.color = new Color(0.88f, 0.94f, 0.98f);
            text.alignment = anchor; text.text = value;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private void ClearButtons()
        {
            foreach (Transform child in panel.transform)
                if (child.GetComponent<Button>() != null) Destroy(child.gameObject);
        }

        private void AddButton(string label, float x, float y, Action action, float width = 290f)
        {
            GameObject go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(panel.transform, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(width, 58); rect.anchoredPosition = new Vector2(x, y);
            Image image = go.GetComponent<Image>(); image.color = new Color(0.08f, 0.32f, 0.43f, 1f);
            Button button = go.GetComponent<Button>();
            button.onClick.AddListener(() => action());
            Text text = MakeText(label, Vector2.zero, new Vector2(width - 12, 52), 19, TextAnchor.MiddleCenter, go.transform);
            text.fontStyle = FontStyle.Bold;
        }

        private void ShowHome()
        {
            menuOpen = true; Time.timeScale = 0f; panel.SetActive(true); ClearButtons();
            title.text = "S K Y W A R D";
            body.text = "FLIGHT SIMULATOR\n\nChoose your aircraft, train your skills,\nand take to the skies.\n\n" + AircraftNames[aircraftIndex] + "\n" + MissionNames[missionIndex];
            AddButton("FLY NOW", 0, -115, CloseMenu, 310);
            AddButton("AIRCRAFT SELECT", -160, -190, ShowAircraft);
            AddButton("FLIGHT SCHOOL", 160, -190, ShowLessons);
            AddButton("MISSIONS", -160, -265, ShowMissions);
            AddButton("WEATHER & SETTINGS", 160, -265, ShowSettings);
            AddButton("CAREER & ACHIEVEMENTS", -160, -330, ShowCareer, 310);
            AddButton("AIRPORT SELECT", 160, -330, ShowAirports, 290);
            status.text = "ESC  MENU / RESUME";
        }

        private void CloseMenu()
        {
            menuOpen = false; panel.SetActive(false); Time.timeScale = 1f;
            if (aircraft != null) aircraft.UseKeyboardControls();
        }

        private void ShowAircraft()
        {
            ClearButtons(); title.text = "AIRCRAFT HANGAR";
            body.text = AircraftNames[aircraftIndex] + "\n\n" + AircraftDescription(aircraftIndex) +
                "\n\nSelection changes mass, wing loading, control response and engine performance.";
            AddButton("◀  PREVIOUS", -160, -165, () => { aircraftIndex = (aircraftIndex + 2) % 3; ShowAircraft(); });
            AddButton("NEXT  ▶", 160, -165, () => { aircraftIndex = (aircraftIndex + 1) % 3; ShowAircraft(); });
            AddButton("SELECT AIRCRAFT", 0, -240, ApplyAircraft, 360);
            AddButton("BACK", 0, -305, ShowHome);
        }

        private string AircraftDescription(int i)
        {
            if (i == 1) return "Touring configuration • 900 kg • 18.5 m² wing • 2,850 N thrust\nStable, efficient and suited to longer navigation flights.";
            if (i == 2) return "Aerobatic configuration • 690 kg • 14.2 m² wing • 3,100 N thrust\nLighter airframe with stronger control authority.";
            return "Primary trainer • 780 kg • 16.2 m² wing • 2,600 N thrust\nBalanced handling for learning take-offs and landings.";
        }

        private void ApplyAircraft()
        {
            if (aircraft == null) return;
            AircraftConfig c = ScriptableObject.CreateInstance<AircraftConfig>();
            c.hideFlags = HideFlags.DontSave;
            c.aircraftId = "skyward-preset-" + aircraftIndex;
            c.displayName = AircraftNames[aircraftIndex];
            c.massKg = aircraftIndex == 1 ? 900f : aircraftIndex == 2 ? 690f : 780f;
            c.wingAreaM2 = aircraftIndex == 1 ? 18.5f : aircraftIndex == 2 ? 14.2f : 16.2f;
            c.wingSpanM = aircraftIndex == 1 ? 11.8f : aircraftIndex == 2 ? 9.4f : 10.9f;
            c.maxThrustN = aircraftIndex == 1 ? 2850f : aircraftIndex == 2 ? 3100f : 2600f;
            c.controlAuthority = aircraftIndex == 2 ? 1.35f : aircraftIndex == 1 ? 0.9f : 1f;
            c.maxLiftCoefficient = aircraftIndex == 1 ? 1.55f : aircraftIndex == 2 ? 1.65f : 1.45f;
            c.stallAngleDeg = aircraftIndex == 2 ? 18f : 16f;
            PlayerPrefs.SetInt("SKYWARD_AIRCRAFT", aircraftIndex);
            SkywardSettings.Data.selectedAircraftId = c.aircraftId;
            SkywardSettings.Save();
            aircraft.Reconfigure(c);
            ApplyVisualPreset(aircraft.transform, aircraftIndex);
            ShowAircraft();
            body.text += "\n\nSELECTED ✓  Return to FLY NOW to use this aircraft.";
        }

        private void ApplyVisualPreset(Transform root, int preset)
        {
            Transform wing = root.Find("Left wing");
            Transform tail = root.Find("Tailplane");
            Transform fuselage = root.Find("Fuselage");
            if (wing != null) wing.localScale = new Vector3(preset == 1 ? 1.083f : preset == 2 ? 0.862f : 1f, 1f, preset == 2 ? 0.862f : 1f);
            if (tail != null) tail.localScale = new Vector3(preset == 2 ? 0.861f : 1f, 1f, 1f);
            Color livery = preset == 1 ? new Color(0.88f, 0.9f, 0.92f) : preset == 2 ? new Color(0.92f, 0.22f, 0.12f) : new Color(0.82f, 0.84f, 0.83f);
            if (fuselage != null && fuselage.GetComponent<Renderer>() != null) fuselage.GetComponent<Renderer>().material.color = livery;
            if (wing != null && wing.GetComponent<Renderer>() != null) wing.GetComponent<Renderer>().material.color = livery;
            if (tail != null && tail.GetComponent<Renderer>() != null) tail.GetComponent<Renderer>().material.color = livery;
        }

        private void ShowLessons()
        {
            ClearButtons(); title.text = "FLIGHT SCHOOL";
            string[] descriptions = {
                "Learn the controls. Release the parking brake, start the engine and taxi straight.",
                "Set take-off power, maintain runway heading and rotate smoothly as speed builds.",
                "Establish a safe climb, reduce pitch and maintain a steady heading and airspeed.",
                "Reduce power, configure flaps and gear, stabilise the approach and touch down gently."
            };
            body.text = LessonNames[lessonIndex] + "\n\n" + descriptions[lessonIndex] +
                "\n\nProgress is tracked live. Complete each lesson objective before advancing.";
            AddButton("◀ PREVIOUS", -160, -165, () => { lessonIndex = (lessonIndex + 3) % 4; ShowLessons(); });
            AddButton("NEXT ▶", 160, -165, () => { lessonIndex = (lessonIndex + 1) % 4; ShowLessons(); });
            AddButton("START LESSON", 0, -240, StartLesson, 330);
            AddButton("BACK", 0, -305, ShowHome);
        }

        private void StartLesson()
        {
            PlayerPrefs.SetInt("SKYWARD_ACTIVE_LESSON", lessonIndex); PlayerPrefs.Save();
            CloseMenu();
        }

        private void ShowMissions()
        {
            ClearButtons(); title.text = "MISSION CONTROL";
            string[] details = {
                "FIRST FLIGHT\nTake off, climb above 300 m and return for a runway landing.",
                "CIRCUIT & LANDING\nComplete a circuit, maintain control and land on the runway.",
                "CROSS-COUNTRY\nClimb above 800 m and fly for at least 180 seconds.",
                "HIGH ALTITUDE\nReach 1,500 m above the airfield.",
                "SOFT TOUCHDOWN\nLand on the runway with a vertical touchdown speed gentler than 2 m/s.",
                "WINDY APPROACH\nLand safely on the runway in the crosswind weather preset."
            };
            body.text = MissionNames[missionIndex] + "\n\n" + details[missionIndex] +
                "\n\nMission selection is saved locally.";
            AddButton("◀ PREVIOUS", -160, -165, () => { missionIndex = (missionIndex + 5) % 6; ShowMissions(); });
            AddButton("NEXT ▶", 160, -165, () => { missionIndex = (missionIndex + 1) % 6; ShowMissions(); });
            AddButton("ACCEPT MISSION", 0, -240, () => { PlayerPrefs.SetInt("SKYWARD_ACTIVE_MISSION", missionIndex); PlayerPrefs.Save(); CloseMenu(); }, 330);
            AddButton("BACK", 0, -305, ShowHome);
        }

        private void RecordFlight()
        {
            flightRecorded = true;
            var profile = SkywardSettings.Data;
            profile.completedFlights += 1;
            if (aircraft.TouchdownOnRunway) profile.successfulLandings += 1;
            SkywardSettings.Save();
            if (profile.completedFlights >= 1) PlayerPrefs.SetInt("SKYWARD_ACH_FIRST_LANDING", 1);
            if (profile.successfulLandings >= 5) PlayerPrefs.SetInt("SKYWARD_ACH_FIVE_LANDINGS", 1);
            if (profile.totalFlightSeconds >= 36000f) PlayerPrefs.SetInt("SKYWARD_ACH_TEN_HOURS", 1);
            if (aircraft.TouchdownOnRunway && aircraft.TouchdownVerticalSpeed > -2f)
                PlayerPrefs.SetInt("SKYWARD_ACH_SOFT_LANDING", 1);
            PlayerPrefs.Save();
        }

        private void ShowCareer()
        {
            ClearButtons(); title.text = "PILOT CAREER";
            var profile = SkywardSettings.Data;
            int missions = 0;
            for (int i = 0; i < 6; i++) missions += PlayerPrefs.GetInt("SKYWARD_MISSION_DONE_" + i, 0);
            body.text = $"PILOT  {profile.playerName}\nTOTAL FLIGHT TIME  {profile.totalFlightSeconds / 3600f:0.0} HOURS\nCOMPLETED FLIGHTS  {profile.completedFlights}\nRUNWAY LANDINGS  {profile.successfulLandings}\nMISSIONS COMPLETED  {missions}/6\n\nACHIEVEMENTS\n{AchievementLine("SKYWARD_ACH_FIRST_LANDING", "First Landing")}\n{AchievementLine("SKYWARD_ACH_FIVE_LANDINGS", "Five Runway Landings")}\n{AchievementLine("SKYWARD_ACH_SOFT_LANDING", "Soft Touchdown")}\n{AchievementLine("SKYWARD_ACH_TEN_HOURS", "Ten Hours in the Air")}";
            AddButton("BACK", 0, -305, ShowHome);
        }

        private string AchievementLine(string key, string name)
            => (PlayerPrefs.GetInt(key, 0) == 1 ? "✓  " : "○  ") + name;

        private void ShowAirports()
        {
            ClearButtons(); title.text = "WORLD AIRPORTS";
            string[] descriptions = {
                "AL KHOR • DESERT\nWarm arid terrain, regional hangars and broad runway surroundings.",
                "COASTAL BAY • SHORELINE\nA coastal airfield with water, promenade and sea-side approach scenery.",
                "HIGHLAND • MOUNTAINS\nElevated terrain, mountain ridges and a challenging visual environment."
            };
            body.text = AirportLabels[airportIndex] + "\n\n" + descriptions[airportIndex] +
                "\n\nSelected airport is loaded on the next new session.";
            AddButton("◀ PREVIOUS", -160, -165, () => { airportIndex = (airportIndex + 2) % 3; ShowAirports(); });
            AddButton("NEXT ▶", 160, -165, () => { airportIndex = (airportIndex + 1) % 3; ShowAirports(); });
            AddButton("SET DEPARTURE AIRPORT", 0, -240, () => {
                PlayerPrefs.SetInt("SKYWARD_ACTIVE_AIRPORT", airportIndex);
                SkywardSettings.Data.selectedAirportId = "skyward-airport-" + airportIndex;
                SkywardSettings.Save();
                Transform oldAirport = environment != null ? environment.transform.parent : null;
                if (oldAirport != null) Destroy(oldAirport.gameObject);
                DemoAirportBuilder.Build(airportIndex);
                environment = FindObjectOfType<SkywardEnvironmentDirector>();
                if (aircraft != null)
                {
                    aircraft.transform.position = new Vector3(0f, 0.58f, -620f);
                    aircraft.transform.rotation = Quaternion.identity;
                    if (aircraft.Body != null) { aircraft.Body.velocity = Vector3.zero; aircraft.Body.angularVelocity = Vector3.zero; }
                    aircraft.ResetFlightState();
                    flightRecorded = false;
                }
                ShowAirports();
                body.text += "\n\nAIRPORT LOADED ✓";
            }, 380);
            AddButton("BACK", 0, -305, ShowHome);
        }

        private void ShowSettings()
        {
            ClearButtons(); title.text = "FLIGHT CONDITIONS";
            body.text = "WEATHER PRESET: " + WeatherNames[weatherIndex] +
                "\n\nMaster volume: " + AudioListener.volume.ToString("0.0") +
                "\n\nGraphics quality: " + QualitySettings.names[QualitySettings.GetQualityLevel()] +
                "\n\nChoose a weather profile or adjust audio and quality below.";
            AddButton("WEATHER ◀", -160, -155, () => { weatherIndex = (weatherIndex + 4) % 5; ApplyWeather(); ShowSettings(); });
            AddButton("WEATHER ▶", 160, -155, () => { weatherIndex = (weatherIndex + 1) % 5; ApplyWeather(); ShowSettings(); });
            AddButton("VOLUME −", -160, -220, () => { AudioListener.volume = Mathf.Clamp01(AudioListener.volume - 0.1f); ShowSettings(); });
            AddButton("VOLUME +", 160, -220, () => { AudioListener.volume = Mathf.Clamp01(AudioListener.volume + 0.1f); ShowSettings(); });
            AddButton("QUALITY", -160, -285, () => { QualitySettings.IncreaseLevel(false); ShowSettings(); });
            AddButton("SAVE & BACK", 160, -285, () => { SkywardSettings.Data.masterVolume = AudioListener.volume; SkywardSettings.Save(); ShowHome(); });
        }

        private void ApplyWeather()
        {
            PlayerPrefs.SetInt("SKYWARD_WEATHER", weatherIndex);
            PlayerPrefs.Save();
            if (environment == null) environment = FindObjectOfType<SkywardEnvironmentDirector>();
            if (environment != null) environment.ApplyWeather(weatherIndex, aircraft);
            else if (aircraft != null) aircraft.SetWind(weatherIndex == 1 ? 8f : weatherIndex == 2 ? 12f : 0f, weatherIndex == 2 ? 90f : 240f);
        }
    }
}