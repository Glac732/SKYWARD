using NUnit.Framework;
using UnityEngine;
using Skyward.Aircraft;

namespace Skyward.Tests
{
    public class AerodynamicModelTests
    {
        private AircraftConfig config;
        [SetUp] public void SetUp() => config = ScriptableObject.CreateInstance<AircraftConfig>();
        [TearDown] public void TearDown() => Object.DestroyImmediate(config);

        private AeroResult Evaluate(Vector3 velocity, Vector3 controls = default, float flaps = 0f, bool gear = true, float throttle = 0f)
            => AerodynamicModel.Evaluate(config, Quaternion.identity, velocity, Vector3.zero, controls, flaps, gear, throttle, 1.225f);

        [Test]
        public void ZeroRelativeWindProducesFiniteForceAndMoment()
        {
            AeroResult result = Evaluate(Vector3.zero);
            Assert.IsFalse(float.IsNaN(result.forceWorld.x));
            Assert.IsFalse(float.IsInfinity(result.forceWorld.magnitude));
            Assert.IsFalse(float.IsNaN(result.momentLocal.x));
        }

        [Test]
        public void DynamicPressureScalesWithSpeedSquared()
        {
            AeroResult slow = Evaluate(Vector3.forward * 20f);
            AeroResult fast = Evaluate(Vector3.forward * 40f);
            Assert.That(fast.dynamicPressure / slow.dynamicPressure, Is.EqualTo(4f).Within(0.001f));
        }

        [Test]
        public void PositiveAngleOfAttackProducesPositiveLift()
        {
            AeroResult result = Evaluate(new Vector3(0f, -4f, 30f));
            Assert.Greater(result.angleOfAttackDeg, 0f);
            Assert.Greater(result.forceWorld.y, 0f);
            Assert.Greater(result.momentLocal.x, 0f, "Positive angle of attack should create a nose-down restoring moment.");
        }

        [Test]
        public void FlapsIncreaseLiftAndDrag()
        {
            AeroResult clean = Evaluate(Vector3.forward * 30f, flaps: 0f);
            AeroResult deployed = Evaluate(Vector3.forward * 30f, flaps: 0.2f);
            Assert.Greater(deployed.liftCoefficient, clean.liftCoefficient);
            Assert.Greater(deployed.dragCoefficient, clean.dragCoefficient);
        }

        [Test]
        public void LandingGearAddsDrag()
        {
            AeroResult clean = Evaluate(Vector3.forward * 35f, gear: false);
            AeroResult extended = Evaluate(Vector3.forward * 35f, gear: true);
            Assert.Greater(extended.dragCoefficient, clean.dragCoefficient);
        }

        [Test]
        public void ThrottleIsClampedByAerodynamicModel()
        {
            AeroResult low = Evaluate(Vector3.forward * 10f, throttle: -5f);
            AeroResult high = Evaluate(Vector3.forward * 10f, throttle: 5f);
            Assert.Less(low.forceWorld.z, high.forceWorld.z);
        }

        [Test]
        public void RollInputProducesRollMoment()
        {
            AeroResult neutral = Evaluate(Vector3.forward * 35f);
            AeroResult commanded = Evaluate(Vector3.forward * 35f, controls: new Vector3(0f, 1f, 0f));
            Assert.Less(commanded.momentLocal.z, neutral.momentLocal.z, "Positive roll input should command a right bank (negative local-Z moment).");
        }
    }
}
