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
using Infiniscryption.Spells.Sigils;

namespace Infiniscryption.Spells.Patchers
{
    public static class SpellCards
    {
        internal static void RegisterCustomCards(Harmony harmony)
        {
            // Create the Kettle
            NewCard.Add(
                "Spell_Kettle_of_Avarice",
                "Kettle of Avarice",
                0, 0,
                new List<CardMetaCategory>() { CardMetaCategory.ChoiceNode, CardMetaCategory.TraderOffer },
                CardComplexity.Advanced,
                CardTemple.Wizard,
                "It allows you to draw two more cards",
                bloodCost: 1,
                hideAttackAndHealth: true,
                defaultTex: AssetHelper.LoadTexture("kettle_of_avarice"),
                specialStatIcon: GlobalSpellAbility.Instance.statIconInfo.iconType,
                specialAbilitiesIdsParam: new List<SpecialAbilityIdentifier>() { GlobalSpellAbility.Instance.id },
                abilityIdsParam: new List<AbilityIdentifier>() { DrawTwoCards.Identifier }
            );

            NewCard.Add(
                "Spell_Anger_of_the_Gods",
                "Anger of the Gods",
                0, 0,
                new List<CardMetaCategory>() { CardMetaCategory.ChoiceNode, CardMetaCategory.TraderOffer, CardMetaCategory.Rare },
                CardComplexity.Advanced,
                CardTemple.Nature,
                "For when nothing else will do the trick",
                bloodCost: 2,
                hideAttackAndHealth: true,
                defaultTex: AssetHelper.LoadTexture("anger_of_all"),
                specialStatIcon: GlobalSpellAbility.Instance.statIconInfo.iconType,
                specialAbilitiesIdsParam: new List<SpecialAbilityIdentifier>() { GlobalSpellAbility.Instance.id },
                abilityIdsParam: new List<AbilityIdentifier>() { DestroyAllCardsOnDeath.Identifier }
            );

            NewCard.Add(
                "Spell_Lightning",
                "Lightning",
                0, 0,
                new List<CardMetaCategory>() { CardMetaCategory.ChoiceNode, CardMetaCategory.TraderOffer },
                CardComplexity.Advanced,
                CardTemple.Tech,
                "A perfectly serviceable amount of damage",
                bloodCost: 1,
                hideAttackAndHealth: true,
                defaultTex: AssetHelper.LoadTexture("lightning_bolt"),
                specialStatIcon: TargetedSpellAbility.Instance.statIconInfo.iconType,
                specialAbilitiesIdsParam: new List<SpecialAbilityIdentifier>() { TargetedSpellAbility.Instance.id },
                abilityIdsParam: new List<AbilityIdentifier>() { DirectDamage.Identifier, DirectDamage.Identifier }
            );

            NewCard.Add(
                "Spell_Backpack",
                "Trip to the Store",
                0, 0,
                new List<CardMetaCategory>() { CardMetaCategory.ChoiceNode, CardMetaCategory.TraderOffer, CardMetaCategory.Rare },
                CardComplexity.Advanced,
                CardTemple.Nature,
                "Send one of your creatures on a trip to the store. Who knows what they will come back with",
                bloodCost: 2,
                hideAttackAndHealth: true,
                defaultTex: AssetHelper.LoadTexture("backpack"),
                specialStatIcon: GlobalSpellAbility.Instance.statIconInfo.iconType,
                specialAbilitiesIdsParam: new List<SpecialAbilityIdentifier>() { GlobalSpellAbility.Instance.id },
                abilities: new List<Ability>() { Ability.RandomConsumable }
            );

            NewCard.Add(
                "Spell_Rot_Healing",
                "Rot Healing",
                0, 0,
                new List<CardMetaCategory>() { CardMetaCategory.ChoiceNode, CardMetaCategory.TraderOffer },
                CardComplexity.Advanced,
                CardTemple.Nature,
                "Restores just a little bit of health",
                bonesCost: 1,
                hideAttackAndHealth: true,
                defaultTex: AssetHelper.LoadTexture("plague_doctor"),
                specialStatIcon: TargetedSpellAbility.Instance.statIconInfo.iconType,
                specialAbilitiesIdsParam: new List<SpecialAbilityIdentifier>() { TargetedSpellAbility.Instance.id },
                abilityIdsParam: new List<AbilityIdentifier>() { DirectHeal.Identifier, DirectHeal.Identifier }
            );

            NewCard.Add(
                "Spell_Dammed_up",
                "Dammed Up",
                0, 0,
                new List<CardMetaCategory>() { CardMetaCategory.ChoiceNode, CardMetaCategory.TraderOffer },
                CardComplexity.Advanced,
                CardTemple.Nature,
                "So many dams...",
                bloodCost: 1,
                hideAttackAndHealth: true,
                defaultTex: AssetHelper.LoadTexture("dammed_up"),
                specialStatIcon: TargetedSpellAbility.Instance.statIconInfo.iconType,
                specialAbilitiesIdsParam: new List<SpecialAbilityIdentifier>() { TargetedSpellAbility.Instance.id },
                abilities: new List<Ability>() { Ability.AllStrike, Ability.CreateDams }
            );

            NewCard.Add(
                "Spell_Irritate",
                "Irritate",
                0, 0,
                new List<CardMetaCategory>() { CardMetaCategory.ChoiceNode, CardMetaCategory.TraderOffer },
                CardComplexity.Advanced,
                CardTemple.Nature,
                "This is what happens when you poke the bear...or wolf",
                bonesCost: 2,
                hideAttackAndHealth: true,
                defaultTex: AssetHelper.LoadTexture("snarling_wolf"),
                specialStatIcon: TargetedSpellAbility.Instance.statIconInfo.iconType,
                specialAbilitiesIdsParam: new List<SpecialAbilityIdentifier>() { TargetedSpellAbility.Instance.id },
                abilityIdsParam: new List<AbilityIdentifier>() { AttackBuff.Identifier, DirectDamage.Identifier }
            );

            NewCard.Add(
                "Spell_Compost",
                "Compost",
                0, 0,
                new List<CardMetaCategory>() { CardMetaCategory.ChoiceNode, CardMetaCategory.TraderOffer },
                CardComplexity.Advanced,
                CardTemple.Nature,
                "Time to recycle those old bones",
                bonesCost: 3,
                hideAttackAndHealth: true,
                defaultTex: AssetHelper.LoadTexture("compost"),
                specialStatIcon: GlobalSpellAbility.Instance.statIconInfo.iconType,
                specialAbilitiesIdsParam: new List<SpecialAbilityIdentifier>() { GlobalSpellAbility.Instance.id },
                abilityIdsParam: new List<AbilityIdentifier>() { DrawTwoCards.Identifier }
            );

            NewCard.Add(
                "Spell_Fetch",
                "Go Fetch",
                0, 0,
                new List<CardMetaCategory>() { CardMetaCategory.ChoiceNode, CardMetaCategory.TraderOffer },
                CardComplexity.Advanced,
                CardTemple.Nature,
                "Good doggy",
                hideAttackAndHealth: true,
                defaultTex: AssetHelper.LoadTexture("wolf_fetch"),
                specialStatIcon: GlobalSpellAbility.Instance.statIconInfo.iconType,
                specialAbilitiesIdsParam: new List<SpecialAbilityIdentifier>() { GlobalSpellAbility.Instance.id },
                abilities: new List<Ability>() { Ability.QuadrupleBones }
            );
        }
    }
}