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
using Infiniscryption.KayceeStarters.Sigils;

namespace Infiniscryption.KayceeStarters.Cards
{
    public static class CustomCards
    {
        public static Trait SIDE_DECK_MARKER = (Trait)5103;

        public enum SideDecks
        {
            Squirrel = 0,
            INF_Squirrel_Reach = 1,
            INF_Bee_Drone = 2,
            INF_Ant_Worker = 3,
            INF_Puppy = 4,
            INF_Spare_Tentacle = 5,
            INF_One_Eyed_Goat = 6
        }

        private static void RegisterCustomAbilities(Harmony harmony)
        {
            // This is the 'Gelatinous' ability
            Gelatinous.Register(harmony);
            DoubleTeeth.Register(harmony);
            DoubleBlood.Register(harmony);
            harmony.PatchAll(typeof(Sigils.TailOnHit));
        }

        internal static void RegisterCustomCards(Harmony harmony)
        {
            // Register the sigils
            RegisterCustomAbilities(harmony);

            // Modify the squirrel
            new CustomCard("Squirrel") { traits = new List<Trait>() { SIDE_DECK_MARKER }};

            // Create the Better Squirrel
            NewCard.Add(
                CustomCards.SideDecks.INF_Squirrel_Reach.ToString(),
                "Squirrel",
                0, 1,
                new List<CardMetaCategory>() { },
                CardComplexity.Vanilla,
                CardTemple.Nature,
                "It's a squirrel that can block fliers.",
                defaultTex: Resources.Load<Texture2D>("art/cards/portraits/portrait_squirrel"),
                pixelTex: Resources.Load<Texture2D>("art/gbc/cards/pixelportraits/pixelportrait_squirrel"),
                abilities: new List<Ability>() { Ability.Reach },
                traits: new List<Trait>() { SIDE_DECK_MARKER }
            );

            // Create the Bee
            NewCard.Add(
                CustomCards.SideDecks.INF_Bee_Drone.ToString(),
                "Bee Drone",
                1, 1,
                new List<CardMetaCategory>() { },
                CardComplexity.Vanilla,
                CardTemple.Nature,
                "For when you need just one point of damage",
                defaultTex: Resources.Load<Texture2D>("art/cards/portraits/portrait_bee"),
                pixelTex: AssetHelper.LoadTexture("pixelportrait_bee"),
                tribes: new List<Tribe>() { Tribe.Insect },
                abilities: new List<Ability>() { Ability.Flying, Ability.Brittle },
                traits: new List<Trait>() { SIDE_DECK_MARKER }
            );

            //Create the Ant
            var antHealthAbility = HealthForAnts.HarmonyInit.antHealthSpecialAbility;
            antHealthAbility.statIconInfo.pixelIconGraphic = Sprite.Create(
                Resources.Load<Texture2D>("art/gbc/cards/pixel_special_stat_icons"),
                new Rect(0f, 27f, 16f, 8f),
                new Vector2(0.5f, 0.5f)
            );
            antHealthAbility.statIconInfo.gbcDescription = "The health of [creature] is equal to the number of Ants that the owner has on their side of the table.";
            NewCard.Add(
                CustomCards.SideDecks.INF_Ant_Worker.ToString(),
                "Ant Drone",
                0, 0,
                new List<CardMetaCategory>() { },
                CardComplexity.Vanilla,
                CardTemple.Nature,
                "It's not much, but it's an ant.",
                defaultTex: AssetHelper.LoadTexture("worker_ant"),
                pixelTex: AssetHelper.LoadTexture("pixelportrait_ant_worker"),
                tribes: new List<Tribe>() { Tribe.Insect },
                traits: new List<Trait>() { Trait.Ant, SIDE_DECK_MARKER },
                specialStatIcon: antHealthAbility.statIconInfo.iconType,
                specialAbilitiesIdsParam: new List<SpecialAbilityIdentifier>() { antHealthAbility.id }
            );

            // Create the Puppy
            NewCard.Add(
                CustomCards.SideDecks.INF_Puppy.ToString(),
                "Puppy",
                0, 1,
                new List<CardMetaCategory>() { },
                CardComplexity.Vanilla,
                CardTemple.Nature,
                "This energetic little puppy will dig up a fresh bone every turn",
                defaultTex: AssetHelper.LoadTexture("digging_dog"),
                pixelTex: AssetHelper.LoadTexture("pixel_digging_dog"),
                tribes: new List<Tribe>() { Tribe.Canine },
                abilities: new List<Ability>() { Ability.Strafe, Ability.BoneDigger },
                traits: new List<Trait>() { SIDE_DECK_MARKER }
            );

            // Create the Squid Tail
            NewCard.Add(
                CustomCards.SideDecks.INF_Spare_Tentacle.ToString() + "_Tail",
                "Tentacle",
                0, 2,
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
                CustomCards.SideDecks.INF_Spare_Tentacle.ToString(),
                "Spare Tentacle",
                0, 2,
                new List<CardMetaCategory>() { },
                CardComplexity.Vanilla,
                CardTemple.Nature,
                "I've never seen that [c:bR]thing[c:] before, but I can tell there are no bones in it.",
                defaultTex: AssetHelper.LoadTexture("squid_grunt"),
                pixelTex: AssetHelper.LoadTexture("pixelportrait_squidgrunt"),
                titleGraphic: Resources.Load<Texture2D>("art/cards/special/squid_title"),
                abilities: new List<Ability>() { Ability.TailOnHit },
                abilityIdsParam: new List<AbilityIdentifier>() { Gelatinous.Identifier },
                tailId: new TailIdentifier(
                    CustomCards.SideDecks.INF_Spare_Tentacle.ToString() + "_Tail",
                    AssetHelper.LoadTexture("squid_grunt_taillost")
                ),
                traits: new List<Trait>() { SIDE_DECK_MARKER }
            );

            // Create the Goat
            NewCard.Add(
                CustomCards.SideDecks.INF_One_Eyed_Goat.ToString(),
                "One-Eyed Goat",
                0, 1,
                new List<CardMetaCategory>() { },
                CardComplexity.Vanilla,
                CardTemple.Nature,
                "This goat generates additional blood...for a price",
                defaultTex: AssetHelper.LoadTexture("portrait_goat_double"),
                pixelTex: AssetHelper.LoadTexture("pixelportrait_one_eyed_goat"),
                abilityIdsParam: new List<AbilityIdentifier>() { DoubleTeeth.Identifier, DoubleBlood.Identifier },
                traits: new List<Trait>() { SIDE_DECK_MARKER }
            );
        }
    }
}