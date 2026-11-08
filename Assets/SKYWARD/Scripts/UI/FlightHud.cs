using UnityEngine;
using UnityEngine.UI;
using Skyward.Aircraft;

namespace Skyward.UI
{
    public sealed class FlightHud : MonoBehaviour
    {
        [SerializeField] private AircraftController aircraft;
        private Text readout;
        private Text help;
        private Image panel;

        public void SetAircraft(AircraftController value) => aircraft = value;

        private void Awake()
        {
            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;
            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            gameObject.AddComponent<GraphicRaycaster>();

            GameObject backing = new GameObject("Flight data panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            backing.transform.SetParent(transform, false);
            RectTransform backingRect = backing.GetComponent<RectTransform>();
            backingRect.anchorMin = new Vector2(0f, 1f);
            backingRect.anchorMax = new Vector2(0f, 1f);
            backingRect.pivot = new Vector2(0f, 1f);
            backingRect.anchoredPosition = new Vector2(16f, -16f);
            backingRect.sizeDelta = new Vector2(390f, 286f);
            panel = backing.GetComponent<Image>();
            panel.color = new Color(0.025f, 0.055f, 0.075f, 0.78f);

            readout = CreateText("Live flight telemetry", new Vector2(34f, -30f), new Vector2(350f, 248f), 19, TextAnchor.UpperLeft, backing.transform);
            help = CreateText("Controls", new Vector2(20f, 20f), new Vector2(1050f, 34f), 15, TextAnchor.LowerLeft, transform);
            help.text = "W/S PITCH   A/D ROLL   Q/E RUDDER   SHIFT/CTRL THROTTLE   SPACE BRAKES   B PARKING BRAKE   G GEAR   F FLAPS   X ENGINE   C CAMERA   ESC PAUSE";
        }

        private Text CreateText(string objectName, Vector2 anchoredPosition, Vector2 size, int fontSize, TextAnchor anchor, Transform parent)
        {
            GameObject go = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            Text text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = fontSize;
            text.color = Color.white;
            text.alignment = anchor;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private void Update()
        {
            if (aircraft == null || readout == null) return;
            AeroResult aero = aircraft.LastAero;
            readout.text =
                "SKYWARD  /  LIVE FLIGHT DATA\n" +
                $"IAS       {aircraft.AirspeedKnots,5:000} kt\n" +
                $"GROUND    {aircraft.GroundSpeedKnots,5:000} kt\n" +
                $"ALT       {aircraft.AltitudeMeters,6:0000} m AGL\n" +
                $"V/S       {aircraft.VerticalSpeedMps,+0.0;-0.0;0.0} m/s\n" +
                $"HDG       {aircraft.HeadingDegrees,5:000}°\n" +
                $"PITCH     {aircraft.PitchDegrees,+0.0;-0.0;0.0}°   BANK {aircraft.BankDegrees,+0.0;-0.0;0.0}°\n" +
                $"AOA       {aero.angleOfAttackDeg,+0.0;-0.0;0.0}°   BETA {aero.sideslipDeg,+0.0;-0.0;0.0}°\n" +
                $"THROTTLE  {aircraft.Throttle * 100f,3:000}%   FUEL {aircraft.FuelKg:0.0} kg\n" +
                $"LIFT CL   {aero.liftCoefficient:0.00}   GEAR {(aircraft.GearDown ? "DOWN" : "UP")}\n" +
                $"FLAPS     {aircraft.FlapSetting * 100f:0}%   BRAKES {(aircraft.ParkingBrake ? "PARK" : aircraft.IsGrounded ? "READY" : "AIR")}\n" +
                $"ENGINE    {(aircraft.EngineRunning ? "RUNNING" : "OFF")}   STATE {aircraft.FlightPhase}";
        }
    }
}