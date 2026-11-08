using UnityEngine;

namespace Skyward.Aircraft
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class AircraftController : MonoBehaviour
    {
        [Header("Aircraft")]
        [SerializeField] private AircraftConfig config;
        [SerializeField] private float inputSmoothing = 5.5f;
        [SerializeField] private float throttleRate = 0.28f;
        [SerializeField] private float windSpeedMps;
        [SerializeField] private float windFromDegrees;

        [Header("Landing gear wheel locations (trainer local metres)")]
        [SerializeField] private Vector3 leftGearPosition = new Vector3(-1.25f, -0.68f, -0.2f);
        [SerializeField] private Vector3 rightGearPosition = new Vector3(1.25f, -0.68f, -0.2f);
        [SerializeField] private Vector3 noseGearPosition = new Vector3(0f, -0.68f, 1.65f);

        private Rigidbody body;
        private Vector3 controls;
        private bool externalControlOverride;
        private float throttle = 0.12f;
        private float fuelKg;
        private bool gearDown = true;
        private bool brakes;
        private bool parkingBrake = true;
        private bool engineRunning = true;
        private float flapSetting;
        private WheelCollider leftWheel;
        private WheelCollider rightWheel;
        private WheelCollider noseWheel;
        private Transform leftWheelVisual;
        private Transform rightWheelVisual;
        private Transform noseWheelVisual;
        private Transform propeller;
        private AeroResult lastAero;
        private float flightSeconds;
        private float maximumAltitude;
        private float maximumAirspeed;
        private bool hasLeftGround;
        private bool hasTouchedDown;
        private bool touchdownOnRunway;
        private float touchdownVerticalSpeed;

        public AircraftConfig Config => config;
        public Rigidbody Body => body;
        public float Throttle => throttle;
        public float FuelKg => fuelKg;
        public bool GearDown => gearDown;
        public bool EngineRunning => engineRunning;
        public bool ParkingBrake => parkingBrake;
        public float FlapSetting => flapSetting;
        public float FlightSeconds => flightSeconds;
        public AeroResult LastAero => lastAero;
        public float AirspeedKnots => lastAero.indicatedAirspeed * 1.943844f;
        public float GroundSpeedKnots => body != null ? body.velocity.magnitude * 1.943844f : 0f;
        public float AltitudeMeters => Mathf.Max(0f, transform.position.y + 0.42f);
        public float VerticalSpeedMps => body != null ? body.velocity.y : 0f;
        public float HeadingDegrees => transform.eulerAngles.y;
        public float BankDegrees => Vector3.SignedAngle(Vector3.up, transform.up, transform.forward);
        public bool IsGrounded { get; private set; }
        public bool HasTakenOff => hasLeftGround;
        public bool HasLanded => hasTouchedDown;
        public bool TouchdownOnRunway => touchdownOnRunway;
        public bool IsOnRunway { get; private set; }
        public float TouchdownVerticalSpeed => touchdownVerticalSpeed;
        public float MaximumAltitude => maximumAltitude;
        public float MaximumAirspeed => maximumAirspeed;
        public float PitchDegrees => Mathf.DeltaAngle(0f, transform.eulerAngles.x);
        public string FlightPhase => !hasLeftGround ? (IsGrounded ? "GROUND ROLL" : "PREFLIGHT") : (!IsGrounded ? (VerticalSpeedMps > 1f ? "CLIMB / FLIGHT" : VerticalSpeedMps < -1f ? "DESCENT / APPROACH" : "CRUISE") : "LANDED / ROLL-OUT");

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.useGravity = true;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.maxAngularVelocity = 3.5f;
            body.centerOfMass = new Vector3(0f, -0.12f, -0.15f);
            fuelKg = config != null ? config.fuelCapacityKg : 0f;
        }

        public void Initialize(AircraftConfig aircraftConfig)
        {
            config = aircraftConfig;
            if (body == null) body = GetComponent<Rigidbody>();
            if (config == null)
            {
                Debug.LogError("SKYWARD: Missing aircraft configuration; flight controller disabled.", this);
                enabled = false;
                return;
            }

            body.mass = config.massKg;
            body.inertiaTensor = config.inertiaTensor;
            body.inertiaTensorRotation = Quaternion.identity;
            fuelKg = Mathf.Clamp(fuelKg <= 0f ? config.fuelCapacityKg : fuelKg, 0f, config.fuelCapacityKg);
            BuildLandingGear();
            CacheVisuals();
            enabled = true;
        }

        private void BuildLandingGear()
        {
            leftWheel = CreateWheel("Left main wheel collider", leftGearPosition, false);
            rightWheel = CreateWheel("Right main wheel collider", rightGearPosition, false);
            noseWheel = CreateWheel("Nose wheel collider", noseGearPosition, true);
        }

        private WheelCollider CreateWheel(string wheelName, Vector3 localPosition, bool steering)
        {
            GameObject wheelObject = new GameObject(wheelName);
            wheelObject.layer = gameObject.layer;
            wheelObject.transform.SetParent(transform, false);
            wheelObject.transform.localPosition = localPosition;
            wheelObject.transform.localRotation = Quaternion.identity;
            WheelCollider wheel = wheelObject.AddComponent<WheelCollider>();
            wheel.radius = config.wheelRadius;
            wheel.suspensionDistance = config.suspensionDistance;
            wheel.mass = 18f;
            wheel.wheelDampingRate = 0.35f;
            JointSpring spring = wheel.suspensionSpring;
            spring.spring = config.wheelSpring;
            spring.damper = config.wheelDamping;
            spring.targetPosition = 0.52f;
            wheel.suspensionSpring = spring;
            WheelFrictionCurve forward = wheel.forwardFriction;
            forward.extremumSlip = 0.35f;
            forward.extremumValue = 1f;
            forward.asymptoteSlip = 0.8f;
            forward.asymptoteValue = 0.72f;
            forward.stiffness = 1.45f;
            wheel.forwardFriction = forward;
            WheelFrictionCurve sideways = wheel.sidewaysFriction;
            sideways.extremumSlip = 0.22f;
            sideways.extremumValue = 1f;
            sideways.asymptoteSlip = 0.5f;
            sideways.asymptoteValue = 0.75f;
            sideways.stiffness = steering ? 1.8f : 1.65f;
            wheel.sidewaysFriction = sideways;
            return wheel;
        }

        private void CacheVisuals()
        {
            leftWheelVisual = FindChild("Left wheel");
            rightWheelVisual = FindChild("Right wheel");
            noseWheelVisual = FindChild("Nose wheel");
            propeller = FindChild("Propeller");
        }

        private Transform FindChild(string childName)
        {
            foreach (Transform child in GetComponentsInChildren<Transform>(true))
                if (child != transform && child.name == childName) return child;
            return null;
        }

        private void Update()
        {
            if (config == null || Time.timeScale <= 0f) return;

            if (!externalControlOverride)
            {
                float pitch = Axis(KeyCode.W, KeyCode.S);
                float roll = Axis(KeyCode.D, KeyCode.A);
                float yaw = Axis(KeyCode.E, KeyCode.Q);
                Vector3 target = new Vector3(pitch, roll, yaw);
                controls = Vector3.MoveTowards(controls, target, inputSmoothing * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.LeftShift)) throttle = Mathf.MoveTowards(throttle, 1f, throttleRate * Time.deltaTime);
            if (Input.GetKey(KeyCode.LeftControl)) throttle = Mathf.MoveTowards(throttle, 0f, throttleRate * Time.deltaTime);
            brakes = Input.GetKey(KeyCode.Space);
            if (Input.GetKeyDown(KeyCode.B)) parkingBrake = !parkingBrake;
            if (Input.GetKeyDown(KeyCode.G)) gearDown = !gearDown;
            if (Input.GetKeyDown(KeyCode.X)) engineRunning = !engineRunning;
            if (Input.GetKeyDown(KeyCode.F)) flapSetting = flapSetting >= 0.19f ? 0f : (flapSetting < 0.01f ? 0.10f : 0.20f);
        }

        private static float Axis(KeyCode positive, KeyCode negative)
            => (Input.GetKey(positive) ? 1f : 0f) - (Input.GetKey(negative) ? 1f : 0f);

        private void FixedUpdate()
        {
            if (config == null || body == null) return;
            float dt = Time.fixedDeltaTime;
            flightSeconds += dt;

            Vector3 wind = Quaternion.Euler(0f, windFromDegrees, 0f) * Vector3.forward * windSpeedMps;
            Vector3 relativeAirVelocity = body.velocity - wind;
            float density = Mathf.Clamp(AerodynamicModel.SeaLevelDensity * Mathf.Exp(-Mathf.Max(0f, transform.position.y) / 8500f), 0.25f, 1.225f);
            Vector3 effectiveControls = controls;
            float availableThrottle = engineRunning && fuelKg > 0f ? throttle : 0f;

            lastAero = AerodynamicModel.Evaluate(config, body.rotation, relativeAirVelocity,
                transform.InverseTransformDirection(body.angularVelocity), effectiveControls,
                flapSetting, gearDown, availableThrottle, density);

            body.AddForce(lastAero.forceWorld, ForceMode.Force);
            body.AddRelativeTorque(lastAero.momentLocal * config.controlAuthority, ForceMode.Force);

            SetWheelCollidersEnabled(gearDown);
            if (gearDown) UpdateWheelPhysics();
            else
            {
                IsGrounded = false;
                IsOnRunway = false;
                ApplyRetractedGearDrag();
            }

            if (engineRunning && fuelKg > 0f)
                fuelKg = Mathf.Max(0f, fuelKg - availableThrottle * config.fuelBurnKgPerSecondAtFullPower * dt);

            if (!IsGrounded && transform.position.y > 1.5f) hasLeftGround = true;
            if (hasLeftGround && !hasTouchedDown && IsGrounded)
            {
                hasTouchedDown = true;
                touchdownOnRunway = IsOnRunway;
                touchdownVerticalSpeed = body.velocity.y;
            }
            maximumAltitude = Mathf.Max(maximumAltitude, transform.position.y);
            maximumAirspeed = Mathf.Max(maximumAirspeed, lastAero.airspeed);
        }

        private void SetWheelCollidersEnabled(bool value)
        {
            if (leftWheel != null && leftWheel.enabled != value) leftWheel.enabled = value;
            if (rightWheel != null && rightWheel.enabled != value) rightWheel.enabled = value;
            if (noseWheel != null && noseWheel.enabled != value) noseWheel.enabled = value;
            if (leftWheelVisual != null) leftWheelVisual.gameObject.SetActive(value);
            if (rightWheelVisual != null) rightWheelVisual.gameObject.SetActive(value);
            if (noseWheelVisual != null) noseWheelVisual.gameObject.SetActive(value);
        }

        private void UpdateWheelPhysics()
        {
            WheelHit leftHit = new WheelHit();
            WheelHit rightHit = new WheelHit();
            WheelHit noseHit = new WheelHit();
            bool leftGround = leftWheel != null && leftWheel.GetGroundHit(out leftHit);
            bool rightGround = rightWheel != null && rightWheel.GetGroundHit(out rightHit);
            bool noseGround = noseWheel != null && noseWheel.GetGroundHit(out noseHit);
            IsGrounded = leftGround || rightGround || noseGround;
            IsOnRunway = (leftGround && leftHit.collider != null && leftHit.collider.name == "Runway") ||
                         (rightGround && rightHit.collider != null && rightHit.collider.name == "Runway") ||
                         (noseGround && noseHit.collider != null && noseHit.collider.name == "Runway");

            float brake = (brakes || parkingBrake) ? config.wheelBrakeTorque : 0f;
            if (leftWheel != null) leftWheel.brakeTorque = brake;
            if (rightWheel != null) rightWheel.brakeTorque = brake;
            if (noseWheel != null)
            {
                noseWheel.brakeTorque = brake * 0.7f;
                float speedFactor = Mathf.Clamp01(1f - body.velocity.magnitude / 30f);
                noseWheel.steerAngle = controls.z * 22f * speedFactor * config.steeringAuthority;
            }

            if (IsGrounded && body.velocity.sqrMagnitude > 0.01f)
            {
                Vector3 horizontal = Vector3.ProjectOnPlane(body.velocity, Vector3.up);
                body.AddForce(-horizontal * config.groundDrag, ForceMode.Acceleration);
            }
        }

        private void ApplyRetractedGearDrag()
        {
            if (body.velocity.sqrMagnitude > 1f)
                body.AddForce(-body.velocity.normalized * (body.velocity.sqrMagnitude * 0.0008f), ForceMode.Acceleration);
        }

        private void LateUpdate()
        {
            UpdateWheelVisual(leftWheel, leftWheelVisual);
            UpdateWheelVisual(rightWheel, rightWheelVisual);
            UpdateWheelVisual(noseWheel, noseWheelVisual);
            if (propeller != null && engineRunning)
                propeller.Rotate(Vector3.forward, Mathf.Lerp(300f, 3600f, throttle) * Time.deltaTime, Space.Self);
        }

        private static void UpdateWheelVisual(WheelCollider wheel, Transform visual)
        {
            if (wheel == null || visual == null) return;
            wheel.GetWorldPose(out Vector3 position, out Quaternion rotation);
            visual.position = position;
            visual.rotation = rotation * Quaternion.Euler(90f, 0f, 0f);
        }

        public void SetThrottle(float value) => throttle = Mathf.Clamp01(value);
        public void SetControls(Vector3 value)
        {
            controls = Vector3.ClampMagnitude(value, 1f);
            externalControlOverride = true;
        }
        public void UseKeyboardControls() => externalControlOverride = false;
        public void SetEngineRunning(bool value) => engineRunning = value;
        public void SetGearDown(bool value) => gearDown = value;
        public void ToggleGear() => gearDown = !gearDown;
        public void SetBrakes(bool value) => brakes = value;
        public void SetParkingBrake(bool value) => parkingBrake = value;
        public void SetFlaps(float value) => flapSetting = Mathf.Clamp(value, 0f, 0.3f);
        public void SetWind(float speedMps, float fromDegrees)
        {
            windSpeedMps = Mathf.Clamp(speedMps, 0f, 30f);
            windFromDegrees = Mathf.Repeat(fromDegrees, 360f);
        }
    }
}