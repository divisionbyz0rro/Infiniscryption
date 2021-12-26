using HarmonyLib;
using DiskCardGame;
using Infiniscryption.Core.Helpers;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static class P03AscensionSaveData
    {
        private const string ASCENSION_SAVE_KEY = "CopyOfPart3AscensionSave";
        private const string REGULAR_SAVE_KEY = "CopyOfPart3Save";

        public static bool IsP03Run
        {
            get { return true; }
        }

        public static void EnsureRegularSave()
        {
            // The only way there is not a copy of the regular save is because you went straight to a p03 ascension run
            // after installing the mod. This means that the current part3savedata is your actual act 3 save data
            // We don't want to lose that.
            if (SaveGameHelper.GetValue(REGULAR_SAVE_KEY) == default(string))
                SaveGameHelper.SetValue(REGULAR_SAVE_KEY, SaveManager.ToJSON(Part3SaveData.Data));
        }

        [HarmonyPatch(typeof(Part3SaveData), "Initialize")]
        [HarmonyPrefix]
        private static void ClearSaveData(ref Part3SaveData __instance)
        {
            string saveKey = SaveFile.IsAscension ? ASCENSION_SAVE_KEY : REGULAR_SAVE_KEY;
            SaveGameHelper.SetValue(saveKey, default(string));
        }

        [HarmonyPatch(typeof(SaveManager), "SaveToFile")]
        [HarmonyPrefix]
        public static void SaveBothPart3SaveData()
        {
            // What this does is save a copy of the current part 3 save data somewhere else
            // The idea is that when you play part 3, every time you save we keep a copy of that data
            // And whenever you play ascension part 3, same thing.
            //
            // That way, if you switch over to the other type of part 3, we can load the last time this happened.
            // And whenever creating a new ascension part 3 run, we check to see if there is a copy of part 3 save yet
            // If not, we will end up creating one

            if (SaveFile.IsAscension)
                SaveGameHelper.SetValue(ASCENSION_SAVE_KEY, SaveManager.ToJSON(Part3SaveData.Data));
            else
                SaveGameHelper.SetValue(REGULAR_SAVE_KEY, SaveManager.ToJSON(Part3SaveData.Data));
        }

        [HarmonyPatch(typeof(SaveManager), "LoadFromFile")]
        [HarmonyPostfix]
        [HarmonyAfter(new string[] { "cyantist.inscryption.api" })]
        public static void LoadPart3AscensionSaveData()
        {
            string saveKey = SaveFile.IsAscension ? ASCENSION_SAVE_KEY : REGULAR_SAVE_KEY;
            string part3Data = SaveGameHelper.GetValue(saveKey);
            if (part3Data != default(string))
                SaveManager.SaveFile.part3Data = SaveManager.FromJSON<Part3SaveData>(part3Data);
        }
    }
}