using HarmonyLib;
using DiskCardGame;
using InscryptionAPI.Saves;
using UnityEngine;
using System;
using GBC;
using System.Collections.Generic;

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

        [HarmonyPatch(typeof(AscensionStartScreen), "RunExists", MethodType.Getter)]
        [HarmonyPrefix]
        public static bool DoesP03RunExist(ref bool __result)
        {
            // If we have a Part 3 Ascension Run saved, then yes - a P03 run exists
            if (ModdedSaveManager.SaveData.GetValue(InfiniscryptionP03Plugin.PluginGuid, P03AscensionSaveData.ASCENSION_SAVE_KEY) != default(string))
            {
                __result = true;
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(AscensionMenuScreens), "SwitchToScreen")]
        [HarmonyPrefix]
        public static void ClearP03SaveOnNewRun(AscensionMenuScreens.Screen screen)
        {
            // I can't easily hook into the 'confirm new run' button
            // But the only only way you get to this screen is by confirming a new run 
            if (screen == AscensionMenuScreens.Screen.StarterDeckSelect)
            {
                ModdedSaveManager.SaveData.SetValue(InfiniscryptionP03Plugin.PluginGuid, P03AscensionSaveData.ASCENSION_SAVE_KEY, default(string));
            }

            if (screen == AscensionMenuScreens.Screen.Start) // At the main screen, you can't be in any style of run. Not yet.
            {
                P03AscensionSaveData.IsP03Run = false;
                RunBasedHoloMap.ClearWorldData();
            }
        }

        private static readonly string[] menuItems = new string[] { "Menu_New", "Continue", "Menu_Stats", "Menu_Unlocks", "Menu_Exit", "Menu_QuitApp" };
        [HarmonyPatch(typeof(AscensionMenuScreens), "Start")]
        [HarmonyPostfix]
        public static void AddP03StartOption()
        {
            Traverse menuScreens = Traverse.Create(AscensionMenuScreens.Instance);
            GameObject startScreen = menuScreens.Field("startScreen").GetValue<GameObject>();

            GameObject newButton = startScreen.transform.Find($"Center/MenuItems/{menuItems[0]}").gameObject;
            newButton.GetComponentInChildren<PixelText>().SetText("- NEW LESHY RUN -");
            AscensionMenuInteractable newButtonController = newButton.GetComponent<AscensionMenuInteractable>();

            Vector3 newP03RunPos = startScreen.transform.Find($"Center/MenuItems/{menuItems[1]}").localPosition;
            float ygap = newP03RunPos.y - newButton.transform.localPosition.y;

            // Make room for the new menu option
            for (int i = 1; i < menuItems.Length; i++)
            {
                Transform item = startScreen.transform.Find($"Center/MenuItems/{menuItems[i]}");
                item.localPosition = new Vector3(item.localPosition.x, item.localPosition.y + ygap, item.localPosition.z);
            }

            // Clone the new button
            GameObject newP03Button = GameObject.Instantiate(newButton, newButton.transform.parent);
            newP03Button.transform.localPosition = newP03RunPos;
            newP03Button.name = "Menu_New_P03";
            AscensionMenuInteractable newP03ButtonController = newP03Button.GetComponent<AscensionMenuInteractable>();
            newP03ButtonController.CursorSelectStarted = delegate (MainInputInteractable i) {
                P03AscensionSaveData.IsP03Run = true;
                newButtonController.CursorSelectStart();
            };
            newP03Button.GetComponentInChildren<PixelText>().SetText("- NEW P03 RUN -");

            // Add to transition
            AscensionMenuScreenTransition transitionController = startScreen.GetComponent<AscensionMenuScreenTransition>();
            Traverse transitionTraverse = Traverse.Create(transitionController);
            List<GameObject> onEnableRevealedObjects = transitionTraverse.Field("onEnableRevealedObjects").GetValue<List<GameObject>>();
            List<MainInputInteractable> screenInteractables = transitionTraverse.Field("screenInteractables").GetValue<List<MainInputInteractable>>();

            onEnableRevealedObjects.Insert(onEnableRevealedObjects.IndexOf(newButton) + 1, newP03Button);
            screenInteractables.Insert(screenInteractables.IndexOf(newButtonController) + 1, newP03ButtonController);
        }
    }
}