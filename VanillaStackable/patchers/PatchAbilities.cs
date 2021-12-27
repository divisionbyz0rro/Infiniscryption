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
using System.Linq;
using Infiniscryption.VanillaStackable;

namespace Infiniscryption.VanillaStackable.Patchers
{
    public static class PatchAbilities
    {
        public static Ability[] VANILLA_STACKABLES = new Ability[]
        {
            Ability.BeesOnHit,
            Ability.DrawAnt,
            Ability.DrawCopy,
            Ability.DrawCopyOnDeath,
            Ability.DrawRabbits,
            Ability.DrawRandomCardOnDeath,
            Ability.DrawVesselOnHit,
            Ability.GainBattery,
            Ability.Loot,
            Ability.QuadrupleBones,
            Ability.RandomConsumable,
            Ability.Sentry,
            Ability.Sharp,
            Ability.Tutor,
            Ability.BuffNeighbours,
            Ability.BuffEnemy,
            Ability.BuffGems,
            Ability.DebuffEnemy,
            Ability.ConduitBuffAttack,
            Ability.BuffGems
        };

        private static void MakeVanillaTriggersStackable()
        {
            Traverse trav = Traverse.Create<ScriptableObjectLoader<AbilityInfo>>();
            List<AbilityInfo> allAbilities = trav.Field("allData").GetValue<List<AbilityInfo>>();
            foreach (AbilityInfo info in allAbilities)
            {
                if (VANILLA_STACKABLES.Contains(info.ability))
                    info.canStack = true;
            }
        }

        [HarmonyPatch(typeof(LoadingScreenManager), "LoadGameData")]
        [HarmonyPostfix]
	    public static void MakeAbilitiesStackable_LoadingScreen()
        {
            MakeVanillaTriggersStackable();
        }

        private static int ExcessAbilityCount(this PlayableCard card, Ability ability)
        {
            // Here, we're just counting the excess buffs - the number beyond 1.
            // We know that the original code already counted once
            // We just need to know how many beyond 1 there was

            int count = card.Info.Abilities
                        .Concat(AbilitiesUtil.GetAbilitiesFromMods(card.TemporaryMods))
                        .Where(ab => ab == ability)
                        .Count();

            if (count >= 2)
                return count - 1;
            else
                return 0;
        }

        [HarmonyPatch(typeof(PlayableCard), "GetPassiveAttackBuffs")]
        [HarmonyPostfix]
        public static void MakeAttackBuffsStack(ref int __result, ref PlayableCard __instance)
        {
            // Right now, attack buffs don't stack because this method just checks if the
            // buff exists, and doens't check how many of them you have.
            // Let's change that.
            if (__instance.OnBoard)
            {

                foreach (CardSlot cardSlot in Singleton<BoardManager>.Instance.GetAdjacentSlots(__instance.Slot))
                    if (cardSlot.Card != null)
                        __result += cardSlot.Card.ExcessAbilityCount(Ability.BuffNeighbours);

                // Deal with buff and debuff enemy
                // We have to handle giant cards separately (i.e., the moon)
                if (__instance.Info.HasTrait(Trait.Giant))
                {
                    foreach (CardSlot slot in BoardManager.Instance.GetSlots(true))
                    {
                        if (slot.Card != null)
                        {
                            __result += slot.Card.ExcessAbilityCount(Ability.BuffEnemy) - slot.Card.ExcessAbilityCount(Ability.DebuffEnemy);
                        }
                    }
                }
                else if (__instance.Slot.opposingSlot.Card != null)
                {
                    __result += __instance.Slot.opposingSlot.Card.ExcessAbilityCount(Ability.BuffEnemy) - __instance.Slot.opposingSlot.Card.ExcessAbilityCount(Ability.DebuffEnemy);
                }
                
                // I'm cheating here
                // Who cares about progression data if you're modding
                ProgressionData.SetAbilityLearned(Ability.DebuffEnemy);
                ProgressionData.SetAbilityLearned(Ability.BuffEnemy);
                ProgressionData.SetAbilityLearned(Ability.BuffNeighbours);

                if (ConduitCircuitManager.Instance != null) // No need to check save file location
                {
                    ProgressionData.SetAbilityLearned(Ability.CellBuffSelf);
                    ProgressionData.SetAbilityLearned(Ability.BuffGems);

                    List<PlayableCard> conduitsForSlot = ConduitCircuitManager.Instance.GetConduitsForSlot(__instance.Slot);
                    foreach (PlayableCard conduit in conduitsForSlot)
                        __result += conduit.ExcessAbilityCount(Ability.ConduitBuffAttack);
                }

                if (__instance.Info.HasTrait(Trait.Gem))
                    foreach (CardSlot slot in BoardManager.Instance.GetSlots(!__instance.OpponentCard))
                        if (slot.Card != null)
                            __result += slot.Card.ExcessAbilityCount(Ability.BuffGems);
            }
        }
    }
}