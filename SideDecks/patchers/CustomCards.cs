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
using Infiniscryption.SideDecks.Sigils;

namespace Infiniscryption.SideDecks.Patchers
{
    public static class CustomCards
    {
        private static void RegisterCustomAbilities(Harmony harmony)
        {
            // This is the 'Gelatinous' ability
            Gelatinous.Register(harmony);
            DoubleTeeth.Register(harmony);
            DoubleBlood.Register(harmony);
        }

        internal static void RegisterCustomCards(Harmony harmony)
        {
            // Register the sigils
            RegisterCustomAbilities(harmony);

            // Modify the squirrel
            List<Ability> squirrelAbs = new List<Ability>() { Ability.Reach };
            new CustomCard("Squirrel") { abilities = squirrelAbs };

            // Create the Bee
            NewCard.Add(
                SideDeckPatcher.SideDecks.INF_Bee_Drone.ToString(),
                "Bee Drone",
                1, 1,
                new List<CardMetaCategory>() { },
                CardComplexity.Vanilla,
                CardTemple.Nature,
                "For when you need just one point of damage",
                defaultTex: Resources.Load<Texture2D>("art/cards/portraits/portrait_bee"),
                tribes: new List<Tribe>() { Tribe.Insect },
                abilities: new List<Ability>() { Ability.Flying, Ability.Brittle }
            );

            // Create the Ant
            var antHealthAbility = HealthForAnts.HarmonyInit.antHealthSpecialAbility;
            NewCard.Add(
                SideDeckPatcher.SideDecks.INF_Ant_Worker.ToString(),
                "Worker Ant",
                0, 0,
                new List<CardMetaCategory>() { },
                CardComplexity.Vanilla,
                CardTemple.Nature,
                "It's not much, but it's an ant.",
                defaultTex: AssetHelper.LoadTexture("worker_ant"),
                tribes: new List<Tribe>() { Tribe.Insect },
                traits: new List<Trait>() { Trait.Ant },
                specialStatIcon: antHealthAbility.statIconInfo.iconType,
                specialAbilitiesIdsParam: new List<SpecialAbilityIdentifier>() { antHealthAbility.id }
            );

            // Create the Puppy
            NewCard.Add(
                SideDeckPatcher.SideDecks.INF_Puppy.ToString(),
                "Puppy",
                0, 1,
                new List<CardMetaCategory>() { },
                CardComplexity.Vanilla,
                CardTemple.Nature,
                "This energetic little puppy will dig up a fresh bone every turn",
                defaultTex: AssetHelper.LoadTexture("digging_dog"),
                tribes: new List<Tribe>() { Tribe.Canine },
                abilities: new List<Ability>() { Ability.Strafe, Ability.BoneDigger }
            );

            // Create the Squid Tail
            NewCard.Add(
                SideDeckPatcher.SideDecks.INF_Spare_Tentacle.ToString() + "_Tail",
                "Tentacle",
                0, 1,
                new List<CardMetaCategory>() { },
                CardComplexity.Vanilla,
                CardTemple.Nature,
                "It's disgusting",
                defaultTex: AssetHelper.LoadTexture("squid_tail"),
                titleGraphic: Resources.Load<Texture2D>("art/cards/special/squid_title"),
                abilityIdsParam: new List<AbilityIdentifier>() { Gelatinous.Identifier }
            );

            // Create the Squid
            NewCard.Add(
                SideDeckPatcher.SideDecks.INF_Spare_Tentacle.ToString(),
                "Spare Tentacle",
                0, 1,
                new List<CardMetaCategory>() { },
                CardComplexity.Vanilla,
                CardTemple.Nature,
                "I've never seen that [c:bR]thing[c:] before, but I can tell there are no bones in it.",
                defaultTex: AssetHelper.LoadTexture("squid_grunt"),
                titleGraphic: Resources.Load<Texture2D>("art/cards/special/squid_title"),
                abilities: new List<Ability>() { Ability.TailOnHit },
                abilityIdsParam: new List<AbilityIdentifier>() { Gelatinous.Identifier },
                tailId: new TailIdentifier(
                    SideDeckPatcher.SideDecks.INF_Spare_Tentacle.ToString() + "_Tail",
                    AssetHelper.LoadTexture("squid_grunt_taillost")
                )
            );

            // Create the Puppy
            NewCard.Add(
                SideDeckPatcher.SideDecks.INF_One_Eyed_Goat.ToString(),
                "One-Eyed Goat",
                0, 1,
                new List<CardMetaCategory>() { },
                CardComplexity.Vanilla,
                CardTemple.Nature,
                "This goat generates additional blood...for a price",
                defaultTex: AssetHelper.LoadTexture("portrait_goat_double"),
                abilityIdsParam: new List<AbilityIdentifier>() { DoubleTeeth.Identifier, DoubleBlood.Identifier }
            );
        }
    }
}