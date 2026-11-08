using UnityEngine;

namespace Skyward.CameraSystem
{
    public sealed class FlightCameraRig : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 chaseOffset = new Vector3(0f, 3.2f, -10f);
        [SerializeField] private float followSharpness = 5f;
        [SerializeField] private float lookSharpness = 7f;
        [SerializeField] private Vector3 cockpitOffset = new Vector3(0f, 1.05f, 0.75f);
        private Camera cam;
        private bool cockpit;
        private float yaw;
        private float pitch = 8f;

        public void SetTarget(Transform value) => target = value;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            if (cam == null) cam = gameObject.AddComponent<Camera>();
            cam.fieldOfView = 68f;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.C)) cockpit = !cockpit;
            if (Input.GetMouseButton(1))
            {
                yaw += Input.GetAxis("Mouse X") * 2f;
                pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * 2f, -25f, 65f);
            }
            if (Input.GetAxis("Mouse ScrollWheel") != 0f && !cockpit)
                chaseOffset = chaseOffset.normalized * Mathf.Clamp(chaseOffset.magnitude - Input.GetAxis("Mouse ScrollWheel") * 4f, 5f, 22f);
        }

        private void LateUpdate()
        {
            if (target == null) return;
            Vector3 desiredPosition;
            Quaternion desiredRotation;
            if (cockpit)
            {
                desiredPosition = target.TransformPoint(cockpitOffset);
                desiredRotation = target.rotation * Quaternion.Euler(pitch, yaw, 0f);
            }
            else
            {
                Quaternion orbit = target.rotation * Quaternion.Euler(pitch, yaw, 0f);
                desiredPosition = target.position + orbit * chaseOffset;
                desiredRotation = Quaternion.LookRotation((target.position + target.up * 0.8f) - desiredPosition, target.up);
            }
            float posT = 1f - Mathf.Exp(-followSharpness * Time.deltaTime);
            float rotT = 1f - Mathf.Exp(-lookSharpness * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, posT);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotT);
        }
    }
}