using DiskCardGame;
using HarmonyLib;

namespace Infiniscryption.Helpers
{
    public static class SaveGameHelper
    {
        private const string SaveKey = "Infiniscryption";

        public static bool GetBool(string key)
        {
            string value = GetValue(key);

            if (value == default(string))
                return false;

            return bool.Parse(GetValue(key));
        }

        public static int GetInt(string key, int fallback=default(int))
        {
            string value = GetValue(key);

            if (value == default(string))
                return fallback;

            return int.Parse(GetValue(key));
        }

        public static float GetFloat(string key, float fallback=default(float))
        {
            string value = GetValue(key);

            if (value == default(string))
                return fallback;
                
            return float.Parse(GetValue(key));
        }

        public static string GetValue(string key)
        {
            string keyVal = ProgressionData.Data.introducedConsumables.Find(str => str.StartsWith($"{SaveKey}.{key}"));
            if (keyVal != default(string))
                return keyVal.Replace($"{SaveKey}.{key}=", "");
            return default(string);
        }

        public static void SetValue(string key, string value)
        {
            for (int i = 0; i < ProgressionData.Data.introducedConsumables.Count; i++)
            {
                if (ProgressionData.Data.introducedConsumables[i].StartsWith($"{SaveKey}.{key}"))
                {
                    ProgressionData.Data.introducedConsumables[i] = $"{SaveKey}.{key}={value}";
                    return;
                }
            }
            ProgressionData.Data.introducedConsumables.Add($"{SaveKey}.{key}={value}");
        }

        public static void ClearValue(string key)
        {
            for (int i = 0; i < ProgressionData.Data.introducedConsumables.Count; i++)
            {
                if (ProgressionData.Data.introducedConsumables[i].StartsWith($"{SaveKey}.{key}"))
                {
                    ProgressionData.Data.introducedConsumables.RemoveAt(i);
                    return;
                }
            }
        }

        /*
        [HarmonyPatch(typeof(SaveFile), "CreateNewSaveFile")]
        [HarmonyPrefix]
        public static void ClearAllKeys()
        {
            // This removes everything from the save file related to this mod 
            // when the chapter select menu creates a new part 1 run.
            int i = 0;
            while (i < ProgressionData.Data.introducedConsumables.Count)
            {
                if (ProgressionData.Data.introducedConsumables[i].StartsWith(SaveKey))
                {
                    ProgressionData.Data.introducedConsumables.RemoveAt(i);
                } else {
                    i += 1;                    
                }
            }
        }
        */
    }
}