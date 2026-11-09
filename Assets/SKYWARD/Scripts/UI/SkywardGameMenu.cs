using System;
using UnityEngine;
using UnityEngine.UI;
using Skyward.Aircraft;
using Skyward.Core;
using Skyward.CameraSystem;

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
        private static readonly string[] AircraftNames = { "SKYWARD 152 • TRAINER", "SKYWARD  touring • TOURER", "SKYWARD AERO • AEROBATIC" };
        private static readonly string[] MissionNames = { "FIRST FLIGHT", "CIRCUIT & LANDING", "CROSS-COUNTRY" };
        private static readonly string[] LessonNames = { "01  PREFLIGHT & CONTROLS", "02  TAKE-OFF", "03  CLIMB & LEVEL FLIGHT", "04  APPROACH & LANDING" };
        private static readonly string[] WeatherNames = { "CLEAR • CALM", "BREEZY • 8 m/s", "CROSSWIND • 12 m/s", "DUSK • CALM" };

        public static void Create(AircraftController target)
        {
            GameObject root = new GameObject("SKYWARD Front End");
            SkywardGameMenu menu = root.AddComponent<SkywardGameMenu>();
            menu.aircraft = target;
        }

        private void Start()
        {
            BuildCanvas();
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
                int mission = PlayerPrefs.GetInt("SKYWARD_ACTIVE_MISSION", 0);
                bool complete = mission == 0
                    ? aircraft.HasTakenOff && aircraft.HasLanded && aircraft.TouchdownOnRunway
                    : mission == 1
                        ? aircraft.FlightSeconds >= 90f && aircraft.HasTakenOff && aircraft.HasLanded && aircraft.TouchdownOnRunway
                        : aircraft.MaximumAltitude >= 800f && aircraft.FlightSeconds >= 180f;
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
            p.pivot = new Vector2(0.5f, 0.5f); p.sizeDelta = new Vector2(760, 650);
            panel.GetComponent<Image>().color = new Color(0.015f, 0.035f, 0.055f, 0.96f);

            title = MakeText("SKYWARD", new Vector2(0, 275), new Vector2(700, 65), 42, TextAnchor.MiddleCenter, panel.transform);
            title.fontStyle = FontStyle.Bold;
            body = MakeText("", new Vector2(0, 75), new Vector2(650, 300), 23, TextAnchor.MiddleCenter, panel.transform);
            body.lineSpacing = 1.25f;
            status = MakeText("", new Vector2(0, -345), new Vector2(800, 35), 17, TextAnchor.MiddleCenter, canvasObject.transform);
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
            menuOpen = true; Time.timeScale = 1f; panel.SetActive(true); ClearButtons();
            title.text = "S K Y W A R D";
            body.text = "FLIGHT SIMULATOR\n\nChoose your aircraft, train your skills,\nand take to the skies.\n\n" + AircraftNames[aircraftIndex] + "\n" + MissionNames[missionIndex];
            AddButton("FLY NOW", 0, -115, CloseMenu, 310);
            AddButton("AIRCRAFT SELECT", -160, -190, ShowAircraft);
            AddButton("FLIGHT SCHOOL", 160, -190, ShowLessons);
            AddButton("MISSIONS", -160, -265, ShowMissions);
            AddButton("WEATHER & SETTINGS", 160, -265, ShowSettings);
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
            if (wing != null) wing.localScale = new Vector3(preset == 1 ? 11.8f : preset == 2 ? 9.4f : 10.9f, 0.12f, preset == 2 ? 1.25f : 1.45f);
            if (tail != null) tail.localScale = new Vector3(preset == 2 ? 3.1f : 3.6f, 0.1f, 0.8f);
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
                "CROSS-COUNTRY\nClimb above 800 m and fly for at least 180 seconds."
            };
            body.text = MissionNames[missionIndex] + "\n\n" + details[missionIndex] +
                "\n\nMission selection is saved locally.";
            AddButton("◀ PREVIOUS", -160, -165, () => { missionIndex = (missionIndex + 2) % 3; ShowMissions(); });
            AddButton("NEXT ▶", 160, -165, () => { missionIndex = (missionIndex + 1) % 3; ShowMissions(); });
            AddButton("ACCEPT MISSION", 0, -240, () => { PlayerPrefs.SetInt("SKYWARD_ACTIVE_MISSION", missionIndex); PlayerPrefs.Save(); CloseMenu(); }, 330);
            AddButton("BACK", 0, -305, ShowHome);
        }

        private void ShowSettings()
        {
            ClearButtons(); title.text = "FLIGHT CONDITIONS";
            body.text = "WEATHER PRESET: " + WeatherNames[weatherIndex] +
                "\n\nMaster volume: " + AudioListener.volume.ToString("0.0") +
                "\n\nGraphics quality: " + QualitySettings.names[QualitySettings.GetQualityLevel()] +
                "\n\nChoose a weather profile or adjust audio and quality below.";
            AddButton("WEATHER ◀", -160, -155, () => { weatherIndex = (weatherIndex + 3) % 4; ApplyWeather(); ShowSettings(); });
            AddButton("WEATHER ▶", 160, -155, () => { weatherIndex = (weatherIndex + 1) % 4; ApplyWeather(); ShowSettings(); });
            AddButton("VOLUME −", -160, -220, () => { AudioListener.volume = Mathf.Clamp01(AudioListener.volume - 0.1f); ShowSettings(); });
            AddButton("VOLUME +", 160, -220, () => { AudioListener.volume = Mathf.Clamp01(AudioListener.volume + 0.1f); ShowSettings(); });
            AddButton("QUALITY", -160, -285, () => { QualitySettings.IncreaseLevel(false); ShowSettings(); });
            AddButton("SAVE & BACK", 160, -285, () => { SkywardSettings.Data.masterVolume = AudioListener.volume; SkywardSettings.Save(); ShowHome(); });
        }

        private void ApplyWeather()
        {
            RenderSettings.fog = weatherIndex == 3;
            RenderSettings.fogDensity = weatherIndex == 3 ? 0.0015f : 0.00035f;
            RenderSettings.ambientIntensity = weatherIndex == 3 ? 0.65f : 1f;
            if (weatherIndex == 3) RenderSettings.ambientLight = new Color(0.35f, 0.38f, 0.52f);
            else RenderSettings.ambientLight = new Color(0.72f, 0.78f, 0.86f);
            wind = weatherIndex == 1 ? 8f : weatherIndex == 2 ? 12f : 0f;
            if (aircraft != null) aircraft.SetWind(wind, weatherIndex == 2 ? 90f : 240f);
        }
    }
}