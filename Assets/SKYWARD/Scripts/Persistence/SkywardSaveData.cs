using System;
using UnityEngine;

namespace Skyward.Persistence
{
    [Serializable]
    public sealed class SkywardSaveData
    {
        public const int CurrentVersion = 1;
        public int version = CurrentVersion;
        public string playerName = "Pilot";
        public string selectedAircraftId = "skyward-trainer-01";
        public string selectedAirportId = "skyward-demo-airport";
        public float masterVolume = 1f;
        public float engineVolume = 1f;
        public float environmentVolume = 1f;
        public bool simplifiedHud;
        public float totalFlightSeconds;
        public int completedFlights;
        public int successfulLandings;
        public string lastSavedUtc;
    }

    public static class SkywardSaveSystem
    {
        private const string SaveKey = "SKYWARD_PROFILE_V1";
        private const string BackupKey = "SKYWARD_PROFILE_V1_BACKUP";

        public static SkywardSaveData Load()
        {
            SkywardSaveData data = TryRead(SaveKey);
            if (data == null) data = TryRead(BackupKey);
            return data ?? new SkywardSaveData();
        }

        public static bool Save(SkywardSaveData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            Sanitize(data);
            data.version = SkywardSaveData.CurrentVersion;
            data.lastSavedUtc = DateTime.UtcNow.ToString("o");
            string json = JsonUtility.ToJson(data);
            string existing = PlayerPrefs.GetString(SaveKey, string.Empty);
            if (!string.IsNullOrEmpty(existing)) PlayerPrefs.SetString(BackupKey, existing);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
            return TryRead(SaveKey) != null;
        }

        public static void Reset()
        {
            PlayerPrefs.DeleteKey(SaveKey);
            PlayerPrefs.DeleteKey(BackupKey);
            PlayerPrefs.Save();
        }

        private static SkywardSaveData TryRead(string key)
        {
            if (!PlayerPrefs.HasKey(key)) return null;
            try
            {
                SkywardSaveData data = JsonUtility.FromJson<SkywardSaveData>(PlayerPrefs.GetString(key));
                if (data == null || data.version < 1 || data.version > SkywardSaveData.CurrentVersion) return null;
                Sanitize(data);
                return data;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"SKYWARD: Could not read save '{key}': {exception.Message}");
                return null;
            }
        }

        private static void Sanitize(SkywardSaveData data)
        {
            if (string.IsNullOrWhiteSpace(data.playerName)) data.playerName = "Pilot";
            if (string.IsNullOrWhiteSpace(data.selectedAircraftId)) data.selectedAircraftId = "skyward-trainer-01";
            if (string.IsNullOrWhiteSpace(data.selectedAirportId)) data.selectedAirportId = "skyward-demo-airport";
            data.masterVolume = Mathf.Clamp01(data.masterVolume);
            data.engineVolume = Mathf.Clamp01(data.engineVolume);
            data.environmentVolume = Mathf.Clamp01(data.environmentVolume);
            data.totalFlightSeconds = Mathf.Max(0f, data.totalFlightSeconds);
            data.completedFlights = Mathf.Max(0, data.completedFlights);
            data.successfulLandings = Mathf.Max(0, data.successfulLandings);
        }
    }
}