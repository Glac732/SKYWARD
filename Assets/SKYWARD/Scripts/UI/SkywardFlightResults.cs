using UnityEngine;
using UnityEngine.UI;
using Skyward.Aircraft;

namespace Skyward.UI
{
    /// <summary>Post-landing debrief with transparent, telemetry-based scoring.</summary>
    public sealed class SkywardFlightResults : MonoBehaviour
    {
        private AircraftController aircraft;
        private SkywardGameMenu menu;
        private GameObject panel;
        private Text report;
        private bool shown;

        public static void Create(AircraftController target, SkywardGameMenu gameMenu)
        {
            GameObject root = new GameObject("SKYWARD Flight Results");
            SkywardFlightResults results = root.AddComponent<SkywardFlightResults>();
            results.aircraft = target;
            results.menu = gameMenu;
        }

        private void Start() => Build();

        private void Build()
        {
            GameObject canvasObject = new GameObject("Flight Results Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 130;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            panel = new GameObject("Debrief panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvasObject.transform, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(760, 570);
            panel.GetComponent<Image>().color = new Color(0.012f, 0.03f, 0.045f, 0.97f);

            report = MakeText("FLIGHT DEBRIEF", new Vector2(0, 65), new Vector2(680, 390), 26, panel.transform);
            AddButton("CONTINUE FLYING", -170, -220, () => { panel.SetActive(false); Time.timeScale = 1f; });
            AddButton("RETURN TO MENU", 170, -220, () => { panel.SetActive(false); menu.ReturnToMenu(); });
            panel.SetActive(false);
        }

        private Text MakeText(string value, Vector2 position, Vector2 size, int fontSize, Transform parent)
        {
            GameObject go = new GameObject("Debrief text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size; rect.anchoredPosition = position;
            Text text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize; text.color = new Color(0.88f, 0.94f, 0.98f);
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.text = value;
            return text;
        }

        private void AddButton(string label, float x, float y, UnityEngine.Events.UnityAction action)
        {
            GameObject go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(panel.transform, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(290, 62); rect.anchoredPosition = new Vector2(x, y);
            go.GetComponent<Image>().color = new Color(0.08f, 0.32f, 0.43f, 1f);
            go.GetComponent<Button>().onClick.AddListener(action);
            MakeText(label, Vector2.zero, new Vector2(280, 54), 19, go.transform).fontStyle = FontStyle.Bold;
        }

        public void ResetForNextFlight()
        {
            shown = false;
            if (panel != null) panel.SetActive(false);
        }

        private void Update()
        {
            if (shown || aircraft == null || !aircraft.HasLanded || panel == null) return;
            shown = true;
            int score = CalculateScore();
            string grade = score >= 90 ? "A • EXCELLENT" : score >= 75 ? "B • VERY GOOD" :
                score >= 60 ? "C • SATISFACTORY" : "D • NEEDS PRACTICE";
            report.text =
                "FLIGHT DEBRIEF\n\n" +
                $"LANDING RESULT   {(aircraft.TouchdownOnRunway ? "RUNWAY" : "OFF RUNWAY")}\n" +
                $"LANDING SCORE   {score}/100   {grade}\n\n" +
                $"TOUCHDOWN IAS   {aircraft.TouchdownAirspeedKnots:0} kt\n" +
                $"VERTICAL SPEED   {aircraft.TouchdownVerticalSpeed:+0.0;-0.0;0.0} m/s\n" +
                $"BANK ANGLE   {aircraft.TouchdownBankDegrees:+0.0;-0.0;0.0}°\n" +
                $"PITCH ANGLE   {aircraft.TouchdownPitchDegrees:+0.0;-0.0;0.0}°\n\n" +
                "Score is based on runway contact, touchdown rate,\n" +
                "airspeed and bank angle. This is a game score, not a real-flight assessment.";
            panel.SetActive(true);
            Time.timeScale = 0f;
        }

        private int CalculateScore()
        {
            int score = aircraft.TouchdownOnRunway ? 45 : 0;
            float rate = Mathf.Abs(aircraft.TouchdownVerticalSpeed);
            score += rate < 1.5f ? 30 : rate < 3f ? 22 : rate < 5f ? 10 : 0;
            float bank = Mathf.Abs(aircraft.TouchdownBankDegrees);
            score += bank < 5f ? 15 : bank < 10f ? 8 : 0;
            float speed = aircraft.TouchdownAirspeedKnots;
            score += speed >= 45f && speed <= 75f ? 10 : speed >= 40f && speed <= 85f ? 5 : 0;
            return Mathf.Clamp(score, 0, 100);
        }
    }
}