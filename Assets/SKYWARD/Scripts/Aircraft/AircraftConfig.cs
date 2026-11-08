using UnityEngine;

namespace Skyward.Aircraft
{
    [CreateAssetMenu(menuName = "SKYWARD/Aircraft Configuration", fileName = "TrainerConfig")]
    public sealed class AircraftConfig : ScriptableObject
    {
        [Header("Identity")]
        public string aircraftId = "skyward-trainer-01";
        public string displayName = "SKYWARD Trainer";

        [Header("Mass and Geometry")]
        [Min(100f)] public float massKg = 780f;
        [Min(1f)] public float wingAreaM2 = 16.2f;
        [Min(0.1f)] public float wingSpanM = 10.9f;
        [Min(0.1f)] public float meanChordM = 1.5f;
        public Vector3 inertiaTensor = new Vector3(1100f, 1500f, 2100f);

        [Header("Aerodynamics")]
        public float liftSlopePerRad = 4.8f;
        public float zeroLiftDrag = 0.032f;
        public float oswaldEfficiency = 0.78f;
        public float maxLiftCoefficient = 1.45f;
        public float stallAngleDeg = 16f;
        public float sideForceSlope = 0.72f;
        public float sideDragFactor = 0.8f;
        public float extraDrag = 0.025f;
        public float gearDragCoefficient = 0.018f;
        [Range(0.1f, 2f)] public float controlAuthority = 1f;
        public float elevatorEffectiveness = 0.12f;
        public float rudderEffectiveness = 0.10f;
        public float pitchControlMoment = -0.12f;
        public float pitchDamping = 3.5f;
        public float pitchStability = 0.7f;
        public float rollControlMoment = -0.035f;
        public float rollDamping = 0.65f;
        public float yawControlMoment = 0.035f;
        public float yawDamping = 0.9f;
        public float yawStability = 0.25f;

        [Header("Propulsion")]
        [Min(0f)] public float maxThrustN = 2600f;
        [Min(0f)] public float fuelCapacityKg = 110f;
        [Min(0.001f)] public float fuelBurnKgPerSecondAtFullPower = 0.045f;

        [Header("Landing Gear")]
        public float wheelRadius = 0.32f;
        public float suspensionDistance = 0.22f;
        public float wheelDamping = 4500f;
        public float wheelSpring = 28000f;
        public float wheelBrakeTorque = 1800f;
        public float steeringAuthority = 0.35f;
        public float groundDrag = 0.025f;
    }
}
