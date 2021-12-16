using HarmonyLib;
using DiskCardGame;
using System;
using System.Collections;

namespace Infiniscryption.KayceeStarters.UserInterface
{
    [HarmonyPatch(typeof(AscensionMenuScreens), "ScreenSwitchSequence")]
    public static class AscensionScreenPatchers
    {
        [HarmonyPrefix]
        public static void Prefix(ref AscensionMenuScreens __instance, ref AscensionMenuScreens __state)
        {
            __state = __instance;
        }

        [HarmonyPostfix]
        public static IEnumerator Postfix(IEnumerator sequenceEvent, AscensionMenuScreens __state, AscensionMenuScreens.Screen screen)
        {
            while (sequenceEvent.MoveNext())
                yield return sequenceEvent.Current;

            if ((int)screen == (int)SideDeckSelectorScreen.SIDE_DECK_SCREEN && SideDeckSelectorScreen.Instance != null)
            {
                SideDeckSelectorScreen.Instance.gameObject.transform.SetParent(__state.transform);
                SideDeckSelectorScreen.Instance.gameObject.SetActive(true);
            }
        }
    }
}