using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using DiskCardGame;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System;
using Infiniscryption.Core.Helpers;
using APIPlugin;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public static class ActivatedAbilityIcons
    {
        [HarmonyPatch(typeof(CardAbilityIcons), "SetColorOfDefaultIcons")]
        [HarmonyPostfix]
        public static void MoveAndRecolorActivatedAbilities(ref CardAbilityIcons __instance)
        {
            if (SaveManager.SaveFile.IsPart3)
            {
                Traverse iconTrav = Traverse.Create(__instance);
                List<GameObject> defaultIconGroups = iconTrav.Field("defaultIconGroups").GetValue<List<GameObject>>();
                foreach (GameObject group in defaultIconGroups)
                {
                    if (group.activeSelf)
                    {
                        foreach (AbilityIconInteractable abilityIconInteractable in group.GetComponentsInChildren<AbilityIconInteractable>())
                        {
                            AbilityInfo info = AbilitiesUtil.GetInfo(abilityIconInteractable.Ability);
                            if (info.activated)
                            {
                                abilityIconInteractable.SetColor(Color.white);
                            }
                        }
                    }
                }
            }
        }
    }
}