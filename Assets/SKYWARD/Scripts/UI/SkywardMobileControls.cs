using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Skyward.Aircraft;

namespace Skyward.UI
{
    /// <summary>Touch-first flight controls, enabled only on phones/tablets.</summary>
    public sealed class SkywardMobileControls : MonoBehaviour
    {
        private AircraftController aircraft;
        private float pitch, roll, yaw;

        public static void Create(AircraftController target)
        {
            if (!Application.isMobilePlatform) return;
            GameObject root = new GameObject("SKYWARD Touch Controls");
            SkywardMobileControls controls = root.AddComponent<SkywardMobileControls>();
            controls.aircraft = target;
            controls.Build();
        }

        private void Build()
        {
            GameObject canvasObject = new GameObject("Touch Control Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            AddHoldButton(canvasObject.transform, "PITCH +", new Vector2(0.12f, 0.26f), MobileAxis.Pitch, 1f);
            AddHoldButton(canvasObject.transform, "PITCH −", new Vector2(0.12f, 0.10f), MobileAxis.Pitch, -1f);
            AddHoldButton(canvasObject.transform, "ROLL ◀", new Vector2(0.035f, 0.18f), MobileAxis.Roll, -1f);
            AddHoldButton(canvasObject.transform, "ROLL ▶", new Vector2(0.205f, 0.18f), MobileAxis.Roll, 1f);
            AddHoldButton(canvasObject.transform, "RUDDER ◀", new Vector2(0.79f, 0.10f), MobileAxis.Yaw, -1f);
            AddHoldButton(canvasObject.transform, "RUDDER ▶", new Vector2(0.91f, 0.10f), MobileAxis.Yaw, 1f);
            AddActionButton(canvasObject.transform, "THROTTLE +", new Vector2(0.82f, 0.29f), () => aircraft.SetThrottle(aircraft.Throttle + 0.08f));
            AddActionButton(canvasObject.transform, "THROTTLE −", new Vector2(0.94f, 0.29f), () => aircraft.SetThrottle(aircraft.Throttle - 0.08f));
            AddActionButton(canvasObject.transform, "GEAR", new Vector2(0.82f, 0.48f), aircraft.ToggleGear);
            AddActionButton(canvasObject.transform, "FLAPS", new Vector2(0.94f, 0.48f), () => aircraft.SetFlaps(aircraft.FlapSetting >= 0.19f ? 0f : aircraft.FlapSetting + 0.1f));
            AddHoldButton(canvasObject.transform, "BRAKES", new Vector2(0.88f, 0.65f), MobileAxis.Brake, 1f);
        }

        private void AddHoldButton(Transform parent, string label, Vector2 anchor, MobileAxis axis, float value)
        {
            GameObject go = CreateButton(parent, label, anchor);
            MobileTouchButton touch = go.AddComponent<MobileTouchButton>();
            touch.Initialize(this, axis, value);
        }

        private void AddActionButton(Transform parent, string label, Vector2 anchor, UnityEngine.Events.UnityAction action)
        {
            GameObject go = CreateButton(parent, label, anchor);
            go.GetComponent<Button>().onClick.AddListener(action);
        }

        private GameObject CreateButton(Transform parent, string label, Vector2 anchor)
        {
            GameObject go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = anchor;
            rect.sizeDelta = new Vector2(145, 82);
            Image image = go.GetComponent<Image>();
            image.color = new Color(0.02f, 0.12f, 0.17f, 0.62f);
            GameObject textObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(go.transform, false);
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero; textRect.offsetMax = Vector2.zero;
            Text text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = label; text.fontSize = 17; text.fontStyle = FontStyle.Bold;
            text.color = Color.white; text.alignment = TextAnchor.MiddleCenter;
            return go;
        }

        internal void SetAxis(MobileAxis axis, float value)
        {
            switch (axis)
            {
                case MobileAxis.Pitch: pitch = value; break;
                case MobileAxis.Roll: roll = value; break;
                case MobileAxis.Yaw: yaw = value; break;
                case MobileAxis.Brake: aircraft.SetBrakes(value > 0f); return;
            }
            aircraft.SetControls(new Vector3(pitch, roll, yaw));
        }
    }

    public enum MobileAxis { Pitch, Roll, Yaw, Brake }

    public sealed class MobileTouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        private SkywardMobileControls owner;
        private MobileAxis axis;
        private float value;

        public void Initialize(SkywardMobileControls controls, MobileAxis controlAxis, float controlValue)
        {
            owner = controls; axis = controlAxis; value = controlValue;
        }

        public void OnPointerDown(PointerEventData eventData) => owner.SetAxis(axis, value);
        public void OnPointerUp(PointerEventData eventData) => owner.SetAxis(axis, 0f);
        public void OnPointerExit(PointerEventData eventData) => owner.SetAxis(axis, 0f);
    }
}