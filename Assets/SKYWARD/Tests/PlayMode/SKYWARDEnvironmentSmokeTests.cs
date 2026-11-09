using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Skyward.Aircraft;
using Skyward.Environment;

namespace Skyward.Tests
{
    public class SKYWARDEnvironmentSmokeTests
    {
        [UnityTest]
        public IEnumerator AirportVariantsAndWeatherProfilesBuild()
        {
            for (int airportIndex = 0; airportIndex < DemoAirportBuilder.AirportNames.Length; airportIndex++)
            {
                GameObject airport = DemoAirportBuilder.Build(airportIndex);
                yield return null;
                Assert.IsNotNull(airport);
                Assert.IsTrue(airport.name.Contains(DemoAirportBuilder.AirportNames[airportIndex]));
                SkywardEnvironmentDirector director = Object.FindObjectOfType<SkywardEnvironmentDirector>();
                Assert.IsNotNull(director, "Each airport must create its environment director.");
                AircraftConfig config = ScriptableObject.CreateInstance<AircraftConfig>();
                GameObject aircraftObject = new GameObject("Weather test aircraft");
                aircraftObject.AddComponent<Rigidbody>();
                AircraftController aircraft = aircraftObject.AddComponent<AircraftController>();
                aircraft.Initialize(config);

                for (int weather = 0; weather < 5; weather++)
                {
                    director.ApplyWeather(weather, aircraft);
                    Assert.AreEqual(weather == 3 || weather == 4, RenderSettings.fog,
                        "Fog/low-visibility weather profiles must enable atmospheric visibility effects.");
                }

                Object.Destroy(aircraftObject);
                Object.Destroy(config);
                Object.Destroy(airport);
                yield return null;
            }
            RenderSettings.fog = false;
        }
    }
}