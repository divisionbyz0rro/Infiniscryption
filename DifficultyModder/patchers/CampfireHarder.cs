using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using DiskCardGame;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine.UI;
using Infiniscryption.DifficultyMod.Helpers;
using Infiniscryption.DifficultyMod.Sequences;
using Infiniscryption.Core.Helpers;

namespace Infiniscryption.DifficultyMod.Patchers
{
    public class CampfireHarder : DifficultyModBase
    {
        internal override string Description => "Makes the campfire events a little less of a freebie.";

        public override void Reset()
        {
            // This doesn't need to do anything durin a run.
        }

        [HarmonyPatch(typeof(DialogueDataUtil), "ReadDialogueData")]
        [HarmonyPostfix]
        public static void MoreAccurateDialogue()
        {
            // Here, we replace dialogue from Leshy based on the starter decks plugin being installed
            // And add new dialogue
            DialogueHelper.AddOrModifySimpleDialogEvent("StartLeavingWithoutBoosting", new string[] {
                "sensing their intentions, you ran away"
            });

            DialogueHelper.AddOrModifySimpleDialogEvent("LeftWithoutBoosting", new string[] {
                "'come back!' they called after you",
                "but you did not listen as you ran"
            });

            DialogueHelper.AddOrModifySimpleDialogEvent("StillHungry", new string[] {
                "but they are still hungry",
                "and filled with strength, they demand more"
            });

            DialogueHelper.AddOrModifySimpleDialogEvent("EatConsumable", new string[] {
                "you appease them with a bottle from your collection"
            });

            DialogueHelper.AddOrModifySimpleDialogEvent("UseWeapon", new string[] {
                "enraged, they turn your own weapons against you"
            });

            DialogueHelper.AddOrModifySimpleDialogEvent("NothingTheyWant", new string[] {
                "but you have nothing they want for now"    
            });

            DialogueHelper.AddOrModifySimpleDialogEvent("RunScared", new string[] {
                "you feel lucky to have escaped with your life"    
            });
        }

        [HarmonyPatch(typeof(SpecialNodeHandler), "StartSpecialNodeSequence")]
        [HarmonyPrefix]
        public static bool SendToUpgradeShop(ref SpecialNodeHandler __instance, SpecialNodeData nodeData)
        {
            // This sends the player to the upgrade shop if the triggering node is SpendExcessTeeth
            if (DifficultyManager.IsActive<CampfireHarder>())
            {
                if (nodeData is CardStatBoostNodeData)
                {
                    if (__instance.gameObject.GetComponent<CardStatBoostHardSequencer>() == null)
                    {
                        InfiniscryptionDifficultyModPlugin.Log.LogInfo($"Attaching harder card stat boost sequencer to parent");
                        __instance.gameObject.AddComponent<CardStatBoostHardSequencer>();
                    }

                    InfiniscryptionDifficultyModPlugin.Log.LogInfo($"Starting the shop");
                    __instance.StartCoroutine(__instance.gameObject.GetComponent<CardStatBoostHardSequencer>().StatBoostSequence());
                    return false; // This prevents the rest of the thing from running.
                }
            }
            return true; // This makes the rest of the thing run
        }
    }
}