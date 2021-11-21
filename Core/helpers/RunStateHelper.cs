using DiskCardGame;
using HarmonyLib;

namespace Infiniscryption.Core.Helpers
{
    public static class RunStateHelper
    {
        private const string RunStateKey = "RunState";

        public static bool GetBool(string key)
        {
            return SaveGameHelper.GetBool($"{RunStateKey}.{key}");
        }

        public static int GetInt(string key, int fallback=default(int))
        {
            return SaveGameHelper.GetInt($"{RunStateKey}.{key}");
        }

        public static float GetFloat(string key, float fallback=default(float))
        {
            return SaveGameHelper.GetFloat($"{RunStateKey}.{key}");
        }

        public static string GetValue(string key)
        {
            return SaveGameHelper.GetValue($"{RunStateKey}.{key}");
        }

        public static void SetValue(string key, string value)
        {
            SaveGameHelper.SetValue($"{RunStateKey}.{key}", value);
        }

        public static void ClearValue(string key)
        {
            SaveGameHelper.ClearValue($"{RunStateKey}.{key}");
        }

        [HarmonyPatch(typeof(RunState), "Initialize")]
        [HarmonyPostfix]
        public static void ClearAllRunKeys()
        {
            // This removes everything from the save file related to this mod 
            // when the chapter select menu creates a new part 1 run.
            int i = 0;
            while (i < ProgressionData.Data.introducedConsumables.Count)
            {
                if (ProgressionData.Data.introducedConsumables[i].StartsWith($"{SaveGameHelper.SaveKey}.{RunStateKey}"))
                {
                    ProgressionData.Data.introducedConsumables.RemoveAt(i);
                } else {
                    i += 1;                    
                }
            }
        }
    }
}