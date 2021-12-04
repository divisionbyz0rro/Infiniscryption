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
        internal static void RegisterCustomCards()
        {
            // Create the Kettle
            NewCard.Add(
                "Kettle_of_Avarice",
                "Kettle of Avarice",
                0, 0,
                new List<CardMetaCategory>() { CardMetaCategory.ChoiceNode, CardMetaCategory.TraderOffer },
                CardComplexity.Advanced,
                CardTemple.Nature,
                "It allows you to draw two more cards",
                bloodCost: 1,
                hideAttackAndHealth: true,
                defaultTex: AssetHelper.LoadTexture("kettle_of_avarice"),
                specialStatIcon: GlobalSpellAbility.Instance.statIconInfo.iconType,
                specialAbilitiesIdsParam: new List<SpecialAbilityIdentifier>() { GlobalSpellAbility.Instance.id },
                abilityIdsParam: new List<AbilityIdentifier>() { DrawTwoCards.Identifier }
            );

            NewCard.Add(
                "Anger_of_the_Gods",
                "Anger of the Gods",
                0, 0,
                new List<CardMetaCategory>() { CardMetaCategory.ChoiceNode, CardMetaCategory.TraderOffer, CardMetaCategory.Rare },
                CardComplexity.Advanced,
                CardTemple.Nature,
                "For when nothing else will do the trick",
                bloodCost: 1,
                hideAttackAndHealth: true,
                defaultTex: AssetHelper.LoadTexture("anger_of_all"),
                specialStatIcon: GlobalSpellAbility.Instance.statIconInfo.iconType,
                specialAbilitiesIdsParam: new List<SpecialAbilityIdentifier>() { GlobalSpellAbility.Instance.id },
                abilityIdsParam: new List<AbilityIdentifier>() { DestroyAllCardsOnDeath.Identifier }
            );

            NewCard.Add(
                "Lightning",
                "Lightning",
                2, 0,
                new List<CardMetaCategory>() { CardMetaCategory.ChoiceNode, CardMetaCategory.TraderOffer },
                CardComplexity.Advanced,
                CardTemple.Nature,
                "A perfectly serviceable amount of damage",
                bloodCost: 1,
                hideAttackAndHealth: true,
                defaultTex: AssetHelper.LoadTexture("lightning_bolt"),
                specialStatIcon: TargetedSpellAbility.Instance.statIconInfo.iconType,
                specialAbilitiesIdsParam: new List<SpecialAbilityIdentifier>() { TargetedSpellAbility.Instance.id },
                abilityIdsParam: new List<AbilityIdentifier>() { DirectDamage.Identifier, DirectDamage.Identifier }
            );

            NewCard.Add(
                "Backpack",
                "Trip to the Store",
                0, 0,
                new List<CardMetaCategory>() { CardMetaCategory.ChoiceNode, CardMetaCategory.TraderOffer, CardMetaCategory.Rare },
                CardComplexity.Advanced,
                CardTemple.Nature,
                "Send one of your creatures on a trip to the store. Who knows what they will come back with",
                bloodCost: 1,
                hideAttackAndHealth: true,
                defaultTex: AssetHelper.LoadTexture("backpack"),
                specialStatIcon: GlobalSpellAbility.Instance.statIconInfo.iconType,
                specialAbilitiesIdsParam: new List<SpecialAbilityIdentifier>() { GlobalSpellAbility.Instance.id },
                abilities: new List<Ability>() { Ability.RandomConsumable }
            );
        }
    }
}