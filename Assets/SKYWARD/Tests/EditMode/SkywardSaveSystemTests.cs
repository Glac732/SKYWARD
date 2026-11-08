using NUnit.Framework;
using UnityEngine;
using Skyward.Persistence;

namespace Skyward.Tests
{
    public class SkywardSaveSystemTests
    {
        [SetUp] public void SetUp() => SkywardSaveSystem.Reset();
        [TearDown] public void TearDown() => SkywardSaveSystem.Reset();

        [Test]
        public void SaveAndLoadRoundTripsProfile()
        {
            SkywardSaveData data = new SkywardSaveData { playerName = "Test Pilot", totalFlightSeconds = 123.5f, completedFlights = 2 };
            Assert.IsTrue(SkywardSaveSystem.Save(data));
            SkywardSaveData loaded = SkywardSaveSystem.Load();
            Assert.AreEqual("Test Pilot", loaded.playerName);
            Assert.AreEqual(123.5f, loaded.totalFlightSeconds, 0.001f);
            Assert.AreEqual(2, loaded.completedFlights);
        }

        [Test]
        public void SaveClampsInvalidUserSettings()
        {
            SkywardSaveData data = new SkywardSaveData { masterVolume = 3f, engineVolume = -1f, completedFlights = -10 };
            SkywardSaveSystem.Save(data);
            SkywardSaveData loaded = SkywardSaveSystem.Load();
            Assert.AreEqual(1f, loaded.masterVolume);
            Assert.AreEqual(0f, loaded.engineVolume);
            Assert.AreEqual(0, loaded.completedFlights);
        }
    }
}
