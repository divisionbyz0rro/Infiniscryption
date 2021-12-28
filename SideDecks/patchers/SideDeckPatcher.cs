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
using Infiniscryption.SideDecks.Sequences;
using InscryptionAPI.Saves;

namespace Infiniscryption.SideDecks.Patchers
{
    public static class SideDeckPatcher
    {
        public const int SIDE_DECK_SIZE = 10;

        public enum SideDecks
        {
            Squirrel = 0,
            INF_Bee_Drone = 1,
            INF_Ant_Worker = 2,
            INF_Puppy = 3,
            INF_Spare_Tentacle = 4,
            INF_One_Eyed_Goat = 5
        }

        public static string SelectedSideDeck
        {
            get 
            { 
                string sideDeck = ModdedSaveManager.RunState.GetValue(InfiniscryptionSideDecksPlugin.PluginGuid, "SideDeck.SelectedDeck");
                if (String.IsNullOrEmpty(sideDeck))
                    return SideDecks.Squirrel.ToString();

                return sideDeck; 
            }
            set { ModdedSaveManager.RunState.SetValue(InfiniscryptionSideDecksPlugin.PluginGuid, "SideDeck.SelectedDeck", value.ToString()); }
        }

        [HarmonyPatch(typeof(Part1CardDrawPiles), "SideDeckData", MethodType.Getter)]
        [HarmonyPrefix]
        public static bool ReplaceSideDeck(ref List<CardInfo> __result)
        {
            __result = new List<CardInfo>();
            string selectedDeck = SelectedSideDeck;
            for (int i = 0; i < SIDE_DECK_SIZE; i++)
                __result.Add(CardLoader.GetCardByName(selectedDeck));

            return false;
        }

        [HarmonyPatch(typeof(DialogueDataUtil), "ReadDialogueData")]
        [HarmonyPostfix]
        public static void CurseDialogue()
        {
            // Here, we replace dialogue from Leshy based on the starter decks plugin being installed
            // And add new dialogue
            DialogueHelper.AddOrModifySimpleDialogEvent("SideDeckIntro", new string []
            {
                "here you must choose the creatures that will make up your [c:bR]side deck[c:]"
            });
        }

        [HarmonyPatch(typeof(PaperGameMap), "TryInitializeMapData")]
        [HarmonyPrefix]
        [HarmonyAfter(new string[] { 
            "porta.inscryption.traderstart", 
            "zorro.inscryption.infiniscryption.starterdecks"
        })]
        [HarmonyBefore(new string[] { "zorro.inscryption.infiniscryption.curses", "cyantist.inscryption.extendedmap" })]
        public static void StartWithCurseSelection(ref PaperGameMap __instance)
        {
            // This patch ensures that the map always contains a side deck selector node.

            // Be a good citizen - if you haven't completed the tutorial, this should have no effect:
            if (StoryEventsData.EventCompleted(StoryEvent.TutorialRunCompleted))
            {
                InfiniscryptionSideDecksPlugin.Log.LogInfo($"Testing to add sdiedeck node");
                if (RunState.Run.map == null) // Only do this when the map is empty
                {
                    InfiniscryptionSideDecksPlugin.Log.LogInfo($"Map is null - adding sidedeck node");
                    // Let's start by seeing if we have predefined nodes already
                    // It's unfortunately private
                    Traverse paperMapTraverse = Traverse.Create(__instance);
                    PredefinedNodes predefinedNodes = paperMapTraverse.Method("get_PredefinedNodes").GetValue<PredefinedNodes>();
                    if (predefinedNodes != null)
                    {
                        InfiniscryptionSideDecksPlugin.Log.LogInfo($"Inserting the sidedeck node at the end");
                        predefinedNodes.nodeRows.Add(new List<NodeData>() { CustomNodeHelper.GetNodeData<SideDeckSelectionSequencer>("animated_sidedeck") });
                    } else {
                        InfiniscryptionSideDecksPlugin.Log.LogInfo($"Adding the sidedeck node to start");
                        PredefinedNodes nodes = ScriptableObject.CreateInstance<PredefinedNodes>();
                        nodes.nodeRows.Add(new List<NodeData>() { new NodeData() });
                        nodes.nodeRows.Add(new List<NodeData>() { CustomNodeHelper.GetNodeData<SideDeckSelectionSequencer>("animated_sidedeck") });
                        __instance.PredefinedNodes = nodes;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(RunState), "Initialize")]
        [HarmonyPostfix]
        public static void NoTotemTops(ref RunState __instance) 
        {
            // No totem tops for you! That's too freaking easy.
            __instance.totemTops.Clear();
        }
    }
}