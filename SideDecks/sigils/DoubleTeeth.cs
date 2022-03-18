using UnityEngine;
using DiskCardGame;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using Infiniscryption.Core.Helpers;
using InscryptionAPI.Card;

namespace Infiniscryption.SideDecks.Sigils
{
    public class DoubleTeeth : AbilityBehaviour
    {
		public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        // This ability...does nothing? That's right!

        public static void Register(Harmony harmony)
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Painful Entrance";
            info.rulebookDescription = "[creature] will deal two damage to the player when it enters play.";
            info.canStack = true;
            info.powerLevel = 2;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part1Rulebook };
            info.SetPixelAbilityIcon(AssetHelper.LoadTexture("pixelability_doubleteeth"));

            DoubleTeeth.AbilityID = AbilityManager.Add(
                SideDecksPlugin.PluginGuid,
                info,
                typeof(DoubleTeeth),
                AssetHelper.LoadTexture("ability_doubleteeth")
            ).Id;

            harmony.PatchAll(typeof(DoubleTeeth));
        }

        public override bool RespondsToResolveOnBoard()
        {
            return true;
        }

        public override IEnumerator OnResolveOnBoard()
        {
            yield return LifeManager.Instance.ShowDamageSequence(2, 2, true, 0f, null, 0f);
            yield return new WaitForSeconds(0.5f);
            ViewManager.Instance.SwitchToView(View.Default);
        }
    }
}