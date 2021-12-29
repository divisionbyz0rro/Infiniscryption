using HarmonyLib;
using DiskCardGame;
using InscryptionAPI.Saves;
using Infiniscryption.Core.Helpers;
using System.Text;
using System.IO;
using System.IO.Compression;
using System;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static class P03AscensionSaveData
    {
        private const string ASCENSION_SAVE_KEY = "CopyOfPart3AscensionSave";
        private const string REGULAR_SAVE_KEY = "CopyOfPart3Save";

        private static string SaveKey
        {
            get
            {
                if (SceneLoader.ActiveSceneName == "Ascension_Configure")
                    return ASCENSION_SAVE_KEY;

                if (SceneLoader.ActiveSceneName == SceneLoader.StartSceneName)
                    return REGULAR_SAVE_KEY;

                if (SaveFile.IsAscension)
                    return ASCENSION_SAVE_KEY;

                return REGULAR_SAVE_KEY;
            }
        }

        public static bool IsP03Run
        {
            get { return true; }
        }

        private static string ToCompressedJSON(object data)
        {
            string value = SaveManager.ToJSON(data);
            //InfiniscryptionP03Plugin.Log.LogInfo($"JSON SAVE: {value}");
            var bytes = Encoding.Unicode.GetBytes(value);
            using (MemoryStream input = new MemoryStream(bytes))
            {
                using (MemoryStream output = new MemoryStream())
                {
                    using (GZipStream stream = new GZipStream(output, CompressionLevel.Optimal))
                    {
                        input.CopyTo(stream);
                        //stream.Flush();
                    }
                    string result = Convert.ToBase64String(output.ToArray());
                    //InfiniscryptionP03Plugin.Log.LogInfo($"B64 SAVE: {result}");
                    return result;
                }
            }
        }

        private static T FromCompressedJSON<T>(string data)
        {
            var bytes = Convert.FromBase64String(data);
            using(MemoryStream input = new MemoryStream(bytes))
            {
                using(MemoryStream output = new MemoryStream())
                {
                    using(GZipStream stream = new GZipStream(input, CompressionMode.Decompress))
                    {
                        stream.CopyTo(output);
                        //output.Flush();            
                    }
                    string json = Encoding.Unicode.GetString(output.ToArray());
                    return SaveManager.FromJSON<T>(json);
                }
            }
        }

        public static void EnsureRegularSave()
        {
            // The only way there is not a copy of the regular save is because you went straight to a p03 ascension run
            // after installing the mod. This means that the current part3savedata is your actual act 3 save data
            // We don't want to lose that.
            if (ModdedSaveManager.SaveData.GetValue(InfiniscryptionP03Plugin.PluginGuid, REGULAR_SAVE_KEY) == default(string))
                ModdedSaveManager.SaveData.SetValue(InfiniscryptionP03Plugin.PluginGuid, REGULAR_SAVE_KEY, ToCompressedJSON(Part3SaveData.Data));
        }

        [HarmonyPatch(typeof(Part3SaveData), "Initialize")]
        [HarmonyPrefix]
        private static void ClearSaveData(ref Part3SaveData __instance)
        {
            ModdedSaveManager.SaveData.SetValue(InfiniscryptionP03Plugin.PluginGuid, SaveKey, default(string));
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

            InfiniscryptionP03Plugin.Log.LogInfo($"Saving {SaveKey}");
            ModdedSaveManager.SaveData.SetValue(InfiniscryptionP03Plugin.PluginGuid, SaveKey, ToCompressedJSON(SaveManager.SaveFile.part3Data));
        }

        [HarmonyPatch(typeof(SaveManager), "LoadFromFile")]
        [HarmonyPostfix]
        [HarmonyAfter(new string[] { "cyantist.inscryption.api" })]
        public static void LoadPart3AscensionSaveData()
        {
            string part3Data = ModdedSaveManager.SaveData.GetValue(InfiniscryptionP03Plugin.PluginGuid, SaveKey);
            if (part3Data != default(string))
            {
                Part3SaveData data = FromCompressedJSON<Part3SaveData>(part3Data);
                if (data != null)
                {
                    SaveManager.SaveFile.part3Data = data;
                }
            }
        }

        [HarmonyPatch(typeof(Part3SaveData), "Initialize")]
        [HarmonyPrefix]
        public static void EnsurePart3Saved()
        {
            if (SaveFile.IsAscension)
            {
                // Check to see if there is a part 3 save data yet
                EnsureRegularSave();
            }
        }
    }
}