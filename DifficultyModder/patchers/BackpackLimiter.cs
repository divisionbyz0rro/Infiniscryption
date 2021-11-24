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
    public class BackpackLimiter : CurseBase
    {
        // The purpose of this difficulty mod is to force backpack events to only give you a single 
        // consumable.
        private static string CONSUMABLE_VOID = "EmptyVoidOfNothingness";

        private static bool SuppressItems = false;

        public override string Description => "You can only pick up one item from each backpack event, regardless of how many you currently have.";
        public override string Title => "The Empty Backpack";

        Texture2D _iconTexture = AssetHelper.LoadTexture("backpack_icon");
        public override Texture2D IconTexture => _iconTexture;

        public BackpackLimiter(string id, GetActiveDelegate getActive, SetActiveDelegate setActive) : base(id, getActive, setActive) {}

        [HarmonyPatch(typeof(ItemsManager), "UpdateItems")]
        [HarmonyPrefix]
        public static bool PreventItemGenIfNecessary()
        {
            return !SuppressItems; // If we need to suppress items, stop this from happening.
        }

        [HarmonyPatch(typeof(GainConsumablesSequencer), "RegularGainConsumables")]
        [HarmonyPostfix]
        public static IEnumerator AfterGainConsumables(IEnumerator sequenceEvent)
        {
            if (!CurseManager.IsActive<BackpackLimiter>())
            {
                while (sequenceEvent.MoveNext())
                    yield return sequenceEvent.Current;

                yield break;
            }
            
            // Fill the backpack with junk
            while (RunState.Run.consumables.Count < 2)
            {
                InfiniscryptionCursePlugin.Log.LogInfo("Adding empty to prevent too many backpack items");
                RunState.Run.consumables.Add(CONSUMABLE_VOID);
            }

            SuppressItems = true;

            // We force the progression data to say that we know how to fill up with consumables
            // A bit of a hack since we can't modify the dialogue, so we just make it not play at all.
            ProgressionData.SetMechanicLearned(MechanicsConcept.ChooseConsumables);

            // Then yield everything else
            while (sequenceEvent.MoveNext())
                yield return sequenceEvent.Current;

            SuppressItems = false;

            // Remove the junk from the backpack
            while (RunState.Run.consumables.Contains(CONSUMABLE_VOID))
            {
                InfiniscryptionCursePlugin.Log.LogInfo("Removing empty to prevent too many backpack items");
                RunState.Run.consumables.Remove(CONSUMABLE_VOID);
            }

            ItemsManager.Instance.UpdateItems(false);
        }

        public override void Reset()
        {
            // We don't have to do anything during a run.
            // So this stays empty
        }
    }
}