using UnityEngine;
using UnityEngine.UI;
using Skyward.Aircraft;

namespace Skyward.UI
{
    /// <summary>World-space instrument strip mounted in the trainer cockpit.</summary>
    public sealed class CockpitInstrumentPanel : MonoBehaviour
    {
        [SerializeField] private AircraftController aircraft;
        private Text[] displays;

        public void SetAircraft(AircraftController value) => aircraft = value;

        private void Awake() => BuildPanel();

        private void BuildPanel()
        {
            GameObject canvasObject = new GameObject("Functional cockpit instruments", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            canvasObject.transform.SetParent(transform, false);
            canvasObject.transform.localPosition = new Vector3(0f, 0.36f, 0.78f);
            canvasObject.transform.localRotation = Quaternion.identity;
            canvasObject.transform.localScale = Vector3.one * 0.0012f;

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 5;
            RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(1000f, 250f);
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10f;

            GameObject backing = new GameObject("Instrument panel backing", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            backing.transform.SetParent(canvasObject.transform, false);
            RectTransform backingRect = backing.GetComponent<RectTransform>();
            backingRect.anchorMin = Vector2.zero;
            backingRect.anchorMax = Vector2.one;
            backingRect.offsetMin = Vector2.zero;
            backingRect.offsetMax = Vector2.zero;
            backing.GetComponent<Image>().color = new Color(0.012f, 0.025f, 0.032f, 0.96f);

            displays = new Text[7];
            string[] initial = { "IAS\n--- kt", "ALT\n---- m", "V/S\n0.0 m/s", "HDG\n000°", "PITCH\n0.0°", "BANK\n0.0°", "ENGINE\nOFF" };
            for (int i = 0; i < displays.Length; i++)
            {
                GameObject gauge = new GameObject("Instrument " + i, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                gauge.transform.SetParent(backing.transform, false);
                RectTransform rect = gauge.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(-420f + i * 140f, 0f);
                rect.sizeDelta = new Vector2(132f, 210f);
                Text text = gauge.GetComponent<Text>();
                text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                text.fontSize = 35;
                text.fontStyle = FontStyle.Bold;
                text.alignment = TextAnchor.MiddleCenter;
                text.color = new Color(0.55f, 0.9f, 0.95f);
                text.text = initial[i];
                text.horizontalOverflow = HorizontalWrapMode.Overflow;
                text.verticalOverflow = VerticalWrapMode.Overflow;
                displays[i] = text;
            }
        }

        private void Update()
        {
            if (aircraft == null || displays == null) return;
            displays[0].text = $"IAS\n{aircraft.AirspeedKnots:000} kt";
            displays[1].text = $"ALT\n{aircraft.AltitudeMeters:0000} m";
            displays[2].text = $"V/S\n{aircraft.VerticalSpeedMps:+0.0;-0.0;0.0} m/s";
            displays[3].text = $"HDG\n{aircraft.HeadingDegrees:000}°";
            displays[4].text = $"PITCH\n{aircraft.PitchDegrees:+0.0;-0.0;0.0}°";
            displays[5].text = $"BANK\n{aircraft.BankDegrees:+0.0;-0.0;0.0}°";
            displays[6].text = $"ENGINE\n{(aircraft.EngineRunning ? "ON" : "OFF")}\n{aircraft.Throttle * 100f:000}%";
        }
    }
}