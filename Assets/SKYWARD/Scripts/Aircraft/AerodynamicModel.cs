using UnityEngine;

namespace Skyward.Aircraft
{
    public struct AeroResult
    {
        public Vector3 forceWorld;
        public Vector3 momentLocal;
        public float dynamicPressure;
        public float angleOfAttackDeg;
        public float sideslipDeg;
        public float liftCoefficient;
        public float dragCoefficient;
        public float airspeed;
        public float indicatedAirspeed;
    }

    /// <summary>Body-axis aerodynamic model for the SKYWARD single-engine trainer.
    /// Coefficients are game-simulation estimates, not certified aircraft data.</summary>
    public static class AerodynamicModel
    {
        public const float SeaLevelDensity = 1.225f;
        private const float DegreesToRadians = Mathf.Deg2Rad;

        public static AeroResult Evaluate(AircraftConfig config, Quaternion rotation,
            Vector3 relativeAirVelocityWorld, Vector3 angularVelocityLocal,
            Vector3 controls, float flapSetting, bool gearDown, float throttle, float airDensity)
        {
            Vector3 localVelocity = Quaternion.Inverse(rotation) * relativeAirVelocityWorld;
            float speed = localVelocity.magnitude;
            float forwardSpeed = Mathf.Max(0.1f, localVelocity.z);
            float alpha = Mathf.Atan2(-localVelocity.y, forwardSpeed);
            float beta = Mathf.Atan2(localVelocity.x, Mathf.Max(0.1f, new Vector2(localVelocity.y, localVelocity.z).magnitude));
            float alphaDeg = alpha * Mathf.Rad2Deg;
            float betaDeg = beta * Mathf.Rad2Deg;
            float density = Mathf.Clamp(airDensity, 0.05f, SeaLevelDensity);
            float q = 0.5f * density * speed * speed;

            float stallRad = Mathf.Max(8f, config.stallAngleDeg) * DegreesToRadians;
            float clLinear = config.liftSlopePerRad * (alpha + controls.x * config.elevatorEffectiveness) + Mathf.Clamp01(flapSetting) * 0.72f;
            float cl = Mathf.Clamp(clLinear, -config.maxLiftCoefficient, config.maxLiftCoefficient);
            float absAlpha = Mathf.Abs(alpha);
            if (absAlpha > stallRad)
            {
                float blend = Mathf.Clamp01((absAlpha - stallRad) / (25f * DegreesToRadians));
                float postStall = Mathf.Sign(alpha) * (0.25f + 0.35f * Mathf.Cos(Mathf.Clamp(absAlpha, stallRad, 80f * DegreesToRadians)));
                cl = Mathf.Lerp(cl, postStall, blend);
            }

            float effectiveBeta = beta + controls.z * config.rudderEffectiveness;
            float cy = -config.sideForceSlope * effectiveBeta;
            float aspectRatio = config.wingSpanM * config.wingSpanM / Mathf.Max(0.1f, config.wingAreaM2);
            float oswald = Mathf.Clamp(config.oswaldEfficiency, 0.45f, 0.95f);
            float induced = cl * cl / (Mathf.PI * aspectRatio * oswald);
            float cd = config.zeroLiftDrag + induced + config.extraDrag * Mathf.Clamp01(flapSetting) + 0.012f * Mathf.Abs(controls.y);
            if (gearDown) cd += config.gearDragCoefficient;
            if (absAlpha > stallRad) cd += 0.55f * Mathf.Pow(Mathf.Clamp01((absAlpha - stallRad) / (30f * DegreesToRadians)), 2f);
            cd += config.sideDragFactor * beta * beta;

            Vector3 dragLocal = -localVelocity.normalized * (q * config.wingAreaM2 * cd);
            Vector3 liftDirectionLocal = speed > 0.5f
                ? Vector3.Cross(localVelocity.normalized, Vector3.right).normalized
                : Vector3.up;
            Vector3 liftLocal = liftDirectionLocal * (q * config.wingAreaM2 * cl);
            // Positive local-X sideslip must generate a restoring force toward local -X.
            Vector3 sideLocal = Vector3.right * (q * config.wingAreaM2 * cy);
            float propellerThrustFactor = Mathf.Clamp(1f - speed / 115f, 0.28f, 1f);
            Vector3 thrustLocal = Vector3.forward * (config.maxThrustN * Mathf.Clamp01(throttle) * propellerThrustFactor);
            Vector3 forceWorld = rotation * (dragLocal + liftLocal + sideLocal + thrustLocal);

            float pitchRate = angularVelocityLocal.x * config.meanChordM / (2f * Mathf.Max(speed, 2f));
            float rollRate = angularVelocityLocal.z * config.wingSpanM / (2f * Mathf.Max(speed, 2f));
            float yawRate = angularVelocityLocal.y * config.wingSpanM / (2f * Mathf.Max(speed, 2f));
            float cm = config.pitchControlMoment * controls.x - config.pitchDamping * pitchRate + config.pitchStability * alpha;
            float clRoll = config.rollControlMoment * controls.y - config.rollDamping * rollRate;
            float cn = config.yawControlMoment * controls.z - config.yawDamping * yawRate + config.yawStability * beta;
            Vector3 moment = new Vector3(
                q * config.wingAreaM2 * config.meanChordM * cm,
                q * config.wingAreaM2 * config.wingSpanM * cn,
                q * config.wingAreaM2 * config.wingSpanM * clRoll);

            return new AeroResult
            {
                forceWorld = forceWorld,
                momentLocal = moment,
                dynamicPressure = q,
                angleOfAttackDeg = alphaDeg,
                sideslipDeg = betaDeg,
                liftCoefficient = cl,
                dragCoefficient = cd,
                airspeed = speed,
                indicatedAirspeed = speed * Mathf.Sqrt(density / SeaLevelDensity)
            };
        }
    }
}
