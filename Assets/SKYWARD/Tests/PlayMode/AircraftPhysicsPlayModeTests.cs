using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Skyward.Aircraft;

namespace Skyward.Tests
{
    public class AircraftPhysicsPlayModeTests
    {
        private GameObject runway;
        private GameObject aircraft;
        private AircraftConfig config;
        private AircraftController controller;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            runway = GameObject.CreatePrimitive(PrimitiveType.Cube);
            runway.name = "Runway";
            runway.transform.position = new Vector3(0f, -0.48f, 0f);
            runway.transform.localScale = new Vector3(100f, 0.12f, 1000f);

            config = ScriptableObject.CreateInstance<AircraftConfig>();
            aircraft = new GameObject("SKYWARD trainer test aircraft");
            aircraft.transform.position = new Vector3(0f, 0.58f, 0f);
            Rigidbody body = aircraft.AddComponent<Rigidbody>();
            body.mass = config.massKg;
            controller = aircraft.AddComponent<AircraftController>();
            controller.Initialize(config);

            for (int i = 0; i < 20; i++) yield return new WaitForFixedUpdate();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (aircraft != null) Object.Destroy(aircraft);
            if (runway != null) Object.Destroy(runway);
            if (config != null) Object.Destroy(config);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ThreeWheelCollidersSupportAircraftOnRunway()
        {
            WheelCollider[] wheels = aircraft.GetComponentsInChildren<WheelCollider>();
            Assert.AreEqual(3, wheels.Length, "Trainer must have two main wheels and a steerable nose wheel.");
            Assert.IsTrue(controller.IsGrounded, "Wheel suspension should establish contact with the runway.");
            Assert.IsTrue(controller.IsOnRunway, "Ground contact must identify the runway collider.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator RetractingGearDisablesWheelGroundContact()
        {
            controller.SetGearDown(false);
            yield return new WaitForFixedUpdate();
            foreach (WheelCollider wheel in aircraft.GetComponentsInChildren<WheelCollider>())
                Assert.IsFalse(wheel.enabled, "Retracted landing gear must not continue colliding with the ground.");
            Assert.IsFalse(controller.IsGrounded);
        }


        [UnityTest]
        public IEnumerator TrainerAcceleratesAndBecomesAirborne()
        {
            controller.SetParkingBrake(false);
            controller.SetThrottle(1f);
            controller.SetFlaps(0.2f);
            controller.SetControls(new Vector3(0.65f, 0f, 0f));
            for (int i = 0; i < 1000 && !controller.HasTakenOff; i++)
                yield return new WaitForFixedUpdate();
            Assert.IsTrue(controller.HasTakenOff, $"Trainer did not leave the runway. Speed: {controller.Body.velocity.magnitude:0.0} m/s; altitude: {controller.AltitudeMeters:0.0} m.");
            Assert.Greater(controller.Body.velocity.magnitude, 20f);
        }

        [UnityTest]
        public IEnumerator GearDownApproachRecordsRunwayTouchdown()
        {
            controller.transform.position = new Vector3(0f, 3.5f, 0f);
            controller.Body.velocity = new Vector3(0f, -3f, 15f);
            controller.SetThrottle(0f);
            controller.SetControls(Vector3.zero);
            for (int i = 0; i < 300 && !controller.HasLanded; i++)
                yield return new WaitForFixedUpdate();
            Assert.IsTrue(controller.HasLanded, "Wheel suspension should record touchdown after descending onto the runway.");
            Assert.IsTrue(controller.TouchdownOnRunway, "Touchdown must be attributed to the actual runway collider.");
        }

        [Test]
        public void ControlAxesClampIndependently()
        {
            controller.SetControls(new Vector3(2f, -2f, 0.5f));
            Assert.AreEqual(new Vector3(1f, -1f, 0.5f), controller.ControlInput,
                "Pitch, roll and yaw must each retain their own full-scale input.");
        }

        [UnityTest]
        public IEnumerator TelemetryReadsAircraftRigidBodyVelocity()
        {
            controller.SetParkingBrake(false);
            controller.SetThrottle(0f);
            controller.Body.velocity = Vector3.forward * 30f;
            yield return new WaitForFixedUpdate();
            Assert.Greater(controller.LastAero.airspeed, 20f);
            Assert.Greater(controller.AirspeedKnots, 35f);
        }
    }
}
