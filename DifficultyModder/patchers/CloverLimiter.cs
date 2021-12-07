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
using Infiniscryption.Curses.Helpers;
using Infiniscryption.Core.Helpers;

namespace Infiniscryption.Curses.Patchers
{
    public class CloverLimiter : CurseBase
    {
        private const int MAX_CLOVER_USES = 4;

        public override string Description => $"You can only use the clover {MAX_CLOVER_USES} per rune.";
        public override string Title => "The Wilting Clover";

        Texture2D _iconTexture = AssetHelper.LoadTexture("clover_icon");
        public override Texture2D IconTexture => _iconTexture;

        public CloverLimiter(string id, GetActiveDelegate getActive, SetActiveDelegate setActive) : base(id, getActive, setActive) {}

        public static int CloverUses
        {
            get { return RunStateHelper.GetInt("NumberOfCloverUses", 0); }
            set { RunStateHelper.SetValue("NumberOfCloverUses", value.ToString()); }
        }

        public override void Reset()
        {
            // We don't have to do anything during a run.
            // So this stays empty
        }

        [HarmonyPatch(typeof(StoryEventsData), "EventCompleted")]
        [HarmonyPrefix]
        public static bool PretendNoCloverIfUsedTooMuch(StoryEvent storyEvent, ref bool __result)
        {
            if (CurseManager.IsActive<CloverLimiter>())
            {
                // If you've used the clover too much, we just pretend you've never found it
                // This keeps it from ever showing up where it's not supposed to.
                if (storyEvent == StoryEvent.CloverFound && CloverUses >= MAX_CLOVER_USES)
                {
                    __result = false;
                    return false;
                }
                return true;
            }
            else
            {
                return true;
            }
        }

        private static MainInputInteractable _cloverObject;
        private static MainInputInteractable CloverObject
        {
            get
            {
                if (_cloverObject == null)
                {
                    CardSingleChoicesSequencer sequencer = Traverse.Create(SpecialNodeHandler.Instance).Field("cardChoiceSequencer").GetValue<CardSingleChoicesSequencer>();
                    _cloverObject = Traverse.Create(sequencer).Field("rerollInteractable").GetValue<MainInputInteractable>();
                }

                return _cloverObject;
            }
        }

        private static Texture[] CloverTextures = new Texture[]
        {
            Resources.Load<Texture>("art/assets3d/items/clover/clover_albedo"),
            AssetHelper.LoadTexture("clover_albedo_3"),
            AssetHelper.LoadTexture("clover_albedo_2"),
            AssetHelper.LoadTexture("clover_albedo_1"),
            AssetHelper.LoadTexture("clover_albedo_0")
        };

        [HarmonyPatch(typeof(CardSingleChoicesSequencer), "CardSelectionSequence")]
        [HarmonyPostfix]
        public static IEnumerator VisualCloverUpdate(IEnumerator sequenceEvent)
        {
            // All this does is swap out the texture on the clover before anything happens in the node
            // Everything else on the node behaves like normal.
            // The patch to StoryEventData takes care of the logic of making sure the clover doesn't appear if it's overused.
            if (CurseManager.IsActive<CloverLimiter>())
            {
                Texture newCloverTexture = CloverTextures[Math.Min(CloverUses, CloverTextures.Length - 1)];
                CloverObject.gameObject.GetComponentInChildren<Renderer>().material.SetTexture("_MainTex", newCloverTexture);
            }

            // We aren't actually changing the sequence event.
            // Just the game object.
            while (sequenceEvent.MoveNext())
                yield return sequenceEvent.Current;
        }

        [HarmonyPatch(typeof(CardSingleChoicesSequencer), "OnRerollChoices")]
        [HarmonyPostfix]
        public static void IncreaseCloverUses()
        {
            if (CurseManager.IsActive<CloverLimiter>())
                CloverUses = CloverUses + 1; // Just add another use!
        }
    }
}