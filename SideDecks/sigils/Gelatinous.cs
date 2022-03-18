using DiskCardGame;
using HarmonyLib;
using System.Collections;
using Infiniscryption.Core.Helpers;
using System.Collections.Generic;
using UnityEngine;
using InscryptionAPI.Card;

namespace Infiniscryption.SideDecks.Sigils
{
    public class Gelatinous : AbilityBehaviour
    {
		public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        // This ability...does nothing? That's right!

        public static void Register(Harmony harmony)
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Gelatinous";
            info.rulebookDescription = "[creature] has no bones";
            info.canStack = true;
            info.powerLevel = -1;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part1Rulebook };
            info.SetPixelAbilityIcon(AssetHelper.LoadTexture("pixelability_gelatinous"));

            Gelatinous.AbilityID = AbilityManager.Add(
                SideDecksPlugin.PluginGuid,
                info,
                typeof(Gelatinous),
                AssetHelper.LoadTexture("ability_gelatinous")
            ).Id;

            harmony.PatchAll(typeof(Gelatinous));
        }

        [HarmonyPatch(typeof(ResourcesManager), "AddBones")]
        [HarmonyPostfix]
        public static IEnumerator StopAddingBonesIfGelatinous(IEnumerator sequenceResult, CardSlot slot)
        {
            if (slot != null 
                && slot.Card != null 
                && slot.Card.gameObject != null 
                && slot.Card.gameObject.GetComponent<Gelatinous>() != null)
            {
                yield break;
            }
            else
            {
                while (sequenceResult.MoveNext())
                    yield return sequenceResult.Current;
            }
            
        }
    }
}