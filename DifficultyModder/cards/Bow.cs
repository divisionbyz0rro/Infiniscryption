using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using DiskCardGame;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using Infiniscryption.Core.Helpers;
using APIPlugin;
using System.Linq;

namespace Infiniscryption.Curses.Cards
{
    public static class Bow
    {
        public static void RegisterCardAndAbilities(Harmony harmony)
        {
            // AbilityInfo info = AbilityInfoUtils.CreateInfoWithDefaultSettings(
            //     "Swallow Whole",
            //     "This card swallows cards that it attacks or is attacked by. Swallowed cards are slowly digested until they die. The effects are permanent, but killing this card will rescue the digested creature."
            // );
            // info.powerLevel = 7;

            SpecialAbilityIdentifier spellID = SpecialAbilityIdentifier.GetID("zorro.infiniscryption.sigils.targetedspell", "Spell (Targeted)");
            AbilityIdentifier damageID = AbilityIdentifier.GetID("zorro.infiniscryption.sigils.directdamage", "Direct Damage");
            AbilityIdentifier fishhookID = AbilityIdentifier.GetID("zorro.infiniscryption.sigils.fishhook", "Gain Control");

            NewCard.Add(
                "Trapper_Bow",
                "Bow and Arrow",
                0, 0,
                new List<CardMetaCategory>() {  },
                CardComplexity.Advanced,
                CardTemple.Nature,
                "How am I supposed to win with this?",
                bloodCost: 0,
                hideAttackAndHealth: true,
                defaultTex: AssetHelper.LoadTexture("portrait_bow"),
                specialAbilitiesIdsParam: new List<SpecialAbilityIdentifier>() { spellID },
                abilityIdsParam: new List<AbilityIdentifier>() { damageID, damageID }
            );

            NewCard.Add(
                "Trapper_Capture",
                "Capture",
                0, 0,
                new List<CardMetaCategory>() {  },
                CardComplexity.Advanced,
                CardTemple.Nature,
                "How am I supposed to win with this?",
                bloodCost: 0,
                hideAttackAndHealth: true,
                defaultTex: AssetHelper.LoadTexture("portrait_capture"),
                specialAbilitiesIdsParam: new List<SpecialAbilityIdentifier>() { spellID },
                abilityIdsParam: new List<AbilityIdentifier>() { fishhookID }
            );

            NewCard.Add(
                "Trapper_Spike_Trap",
                "Spike Trap",
                0, 2,
                new List<CardMetaCategory>() {  },
                CardComplexity.Advanced,
                CardTemple.Nature,
                "How am I supposed to win with this?",
                bloodCost: 0,
                hideAttackAndHealth: false,
                traits: new List<Trait>() { Trait.Terrain },
                appearanceBehaviour: new List<CardAppearanceBehaviour.Appearance>() { CardAppearanceBehaviour.Appearance.TerrainBackground, CardAppearanceBehaviour.Appearance.TerrainLayout },
                defaultTex: AssetHelper.LoadTexture("portrait_spike_trap"),
                abilities: new List<Ability>() { Ability.Sharp, Ability.DebuffEnemy }
            );
        }
    }
}
