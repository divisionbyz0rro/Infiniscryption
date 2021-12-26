using HarmonyLib;
using DiskCardGame;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static class ScreenManagement
    {
        [HarmonyPatch(typeof(AscensionMenuScreens), "TransitionToGame")]
        [HarmonyPrefix]
        public static void InitializeP03SaveData(ref AscensionMenuScreens __instance, bool newRun)
        {
            if (newRun && P03AscensionSaveData.IsP03Run)
            {
                // Ensure the old part 3 save data gets saved if it needs to be
                P03AscensionSaveData.EnsureRegularSave();
                Part3SaveData data = new Part3SaveData();
                data.Initialize();
                SaveManager.SaveFile.part3Data = data;
                SaveManager.SaveToFile();
            }
        }

        [HarmonyPatch(typeof(MenuController), "LoadGameFromMenu")]
        [HarmonyPrefix]
        public static bool LoadGameFromMenu(bool newGameGBC)
        {
			if (!newGameGBC && SaveFile.IsAscension && P03AscensionSaveData.IsP03Run)
			{
                SaveManager.LoadFromFile();
				LoadingScreenManager.LoadScene("Part3_Cabin");
                SaveManager.savingDisabled = false;
                return false;
			}
			return true;
        }
    }
}