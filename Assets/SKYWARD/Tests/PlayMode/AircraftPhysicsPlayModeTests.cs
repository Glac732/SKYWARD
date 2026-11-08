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
