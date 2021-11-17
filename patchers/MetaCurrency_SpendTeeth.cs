using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using DiskCardGame;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System;
using Infiniscryption;
using Infiniscryption.Helpers;
using Infiniscryption.Sequences;

namespace Infiniscryption.Patchers
{
    public static partial class MetaCurrencyPatches
    {
        // This class has all the logic for spending teeth.
        // You spend teeth by interacting with the teeth skull
        // Yes, this means no more free teeth.

        [HarmonyPatch(typeof(DialogueDataUtil), "ReadDialogueData")]
        [HarmonyPostfix]
        public static void MoreAccurateDialogue()
        {
            // Here, we replace dialogue from Leshy based on the starter decks plugin being installed
            // And add new dialogue
            DialogueHelper.AddOrModifySimpleDialogEvent("AlertSpendTeeth", "here you may spend your ancestors teeth on upgrades");
            DialogueHelper.AddOrModifySimpleDialogEvent("AlertOnlyForNewRun", "but only those who come after will reap the benefits");
            DialogueHelper.AddOrModifySimpleDialogEvent("UpgradedStarterDecks", "your starting decks have grown in power");
        }

        [HarmonyPatch(typeof(FreeTeethSkull), "OnToothClicked")]
        [HarmonyPrefix]
        public static bool GoToCustomTraderWhenToothClicked()
        {
            // We only interrupt the tooth clicking if starter decks are actually active.
            // If starter decks are inactive, let the skull behave as normal
            if (new InfiniscryptionStarterDecksPlugin().Active)
            {
                // Let's see what happens when you click a tooth now!
                Singleton<GameFlowManager>.Instance.TransitionToGameState(GameState.SpecialCardSequence, SpendExcessTeethNodeData.Instance);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(SpecialNodeHandler), "StartSpecialNodeSequence")]
        [HarmonyPrefix]
        public static bool SendToUpgradeShop(ref SpecialNodeHandler __instance, SpecialNodeData nodeData)
        {
            // This sends the player to the upgrade shop if the triggering node is SpendExcessTeeth
            if (nodeData is SpendExcessTeethNodeData)
			{
                if (__instance.gameObject.GetComponent<SpendExcessTeethSequencer>() == null)
                {
                    InfiniscryptionMetaCurrencyPlugin.Log.LogInfo($"Attaching teeth shop sequencer to parent");
                    __instance.gameObject.AddComponent<SpendExcessTeethSequencer>();
                }

                InfiniscryptionMetaCurrencyPlugin.Log.LogInfo($"Starting the shop");
				__instance.StartCoroutine(__instance.gameObject.GetComponent<SpendExcessTeethSequencer>().SpendExcessTeeth());
				return false; // This prevents the rest of the thing from running.
			}
            return true; // This makes the rest of the thing run
        }
    }
}