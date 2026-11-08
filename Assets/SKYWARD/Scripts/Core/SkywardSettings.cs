using UnityEngine;
using Skyward.Persistence;

namespace Skyward.Core
{
    public static class SkywardSettings
    {
        private static SkywardSaveData data;
        private static bool loaded;

        public static SkywardSaveData Data
        {
            get
            {
                if (!loaded)
                {
                    data = SkywardSaveSystem.Load();
                    loaded = true;
                }
                return data;
            }
        }

        public static void Save()
        {
            SkywardSaveSystem.Save(Data);
            AudioListener.volume = Data.masterVolume;
        }

        public static void Reset()
        {
            SkywardSaveSystem.Reset();
            data = new SkywardSaveData();
            loaded = true;
            AudioListener.volume = data.masterVolume;
        }
    }
}