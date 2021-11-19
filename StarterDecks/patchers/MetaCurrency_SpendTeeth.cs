using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using DiskCardGame;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System;
using Infiniscryption.StarterDecks;
using Infiniscryption.Core.Helpers;
using Infiniscryption.StarterDecks.Sequences;
using TMPro;
using UnityEngine.UI;

namespace Infiniscryption.StarterDecks.Patchers
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
            // Destroy the teeth displayer if it's active
            if (_skullTeethContainer != null)
            {
                GameObject.Destroy(_skullTeethContainer);
                _skullTeethContainer = null;
            }

            // Let's see what happens when you click a tooth now!
            Singleton<GameFlowManager>.Instance.TransitionToGameState(GameState.SpecialCardSequence, SpendExcessTeethNodeData.Instance);
            return false;
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
                    InfiniscryptionStarterDecksPlugin.Log.LogInfo($"Attaching teeth shop sequencer to parent");
                    __instance.gameObject.AddComponent<SpendExcessTeethSequencer>();
                }

                InfiniscryptionStarterDecksPlugin.Log.LogInfo($"Starting the shop");
				__instance.StartCoroutine(__instance.gameObject.GetComponent<SpendExcessTeethSequencer>().SpendExcessTeeth());
				return false; // This prevents the rest of the thing from running.
			}
            return true; // This makes the rest of the thing run
        }

        private static GameObject _skullTeethContainer;
        [HarmonyPatch(typeof(ZoomInteractable), "SetZoomed")]
        [HarmonyPostfix]
        public static void DisplayAvailableTeeth(ref ZoomInteractable __instance)
        {
            // This fires whenever you zoom into anything. What we're looking
            // for is to see if you've zoomed into the skull. If so, display
            // the amount of teeth you can spend.
            InfiniscryptionStarterDecksPlugin.Log.LogInfo($"In 'Zoomed'");
            FreeTeethSkull[] skullChild = __instance.gameObject.GetComponentsInChildren<FreeTeethSkull>();
            if (skullChild != null && skullChild.Length > 0)
            {
                InfiniscryptionStarterDecksPlugin.Log.LogInfo($"Instance is {__instance} with name {__instance.name}");    

                if (__instance.Zoomed && _skullTeethContainer == null)
                {
                    _skullTeethContainer = new GameObject();
                    _skullTeethContainer.transform.SetPositionAndRotation(
                        __instance.gameObject.transform.position,
                        __instance.gameObject.transform.rotation
                    );
                    
                    TextMeshPro text = _skullTeethContainer.gameObject.AddComponent<TextMeshPro>();
                    text.fontSize = 5;
                    text.autoSizeTextContainer = true;
                    text.color = new Color(0.533f, 0.4118f, 0.3255f, 0.8f);


                    text.font = Resources.Load<TMP_FontAsset>("fonts/3d scene fonts/garbageschrift");
                    text.alignment = TextAlignmentOptions.Center;

                    text.transform.position += new Vector3(0.2f, 1.6f, -0.7f);
                    text.transform.rotation = Quaternion.LookRotation(new Vector3(-1f, -0.8f, 1f), Vector3.up);

                    text.text = ExcessTeeth.ToString();
                }
                if (!__instance.Zoomed && _skullTeethContainer != null)
                {
                    GameObject.Destroy(_skullTeethContainer);
                    _skullTeethContainer = null;
                }
            } else {
                InfiniscryptionStarterDecksPlugin.Log.LogInfo($"Was not skull");
            }
        }
    }
}