using UnityEngine;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.Core.Helpers;
using InscryptionAPI.Guid;
using Infiniscryption.SideDecks.Sigils;
using InscryptionAPI.Card;
using Infiniscryption.SideDecks;
using System.Collections.Generic;
using System.Linq;
using Infiniscryption.PackManagement;
using System;
using InscryptionAPI.Helpers;

namespace Infiniscryption.SideDecks.Patchers
{
    public static class CustomCards
    {
        public enum SideDecks
        {
            Squirrel = 0,
            INF_Squirrel_Reach = 1,
            INF_Bee_Drone = 2,
            INF_Ant_Worker = 3,
            INF_Puppy = 4,
            INF_Spare_Tentacle = 5,
            INF_One_Eyed_Goat = 6,
            INF_Amalgam_Egg = 7
        }

        public static readonly List<Ability> sideDeckableAbilities = new List<Ability>() {
            Ability.Reach,
            Ability.DeathShield,
            Ability.Sentry,
            Ability.Sharp,
            Ability.GainBattery
        };

        private static void RegisterCustomAbilities(Harmony harmony)
        {
            // This is the 'Gelatinous' ability
            Gelatinous.Register(harmony);
            DoubleTeeth.Register(harmony);
            DoubleBlood.Register(harmony);
        }

        private static void RegisterMetacategoriesInner()
        {
            PackManager.AddProtectedMetacategory(SideDeckManager.SIDE_DECK);
        }

        private static void RegisterMetacategories()
        {
            try
            {
                RegisterMetacategoriesInner();
            }
            catch (Exception)
            {
                SideDecksPlugin.Log.LogInfo($"Error registering pack manager exception - pack manager plugin not loaded. This is not a problem.");
            }
        }

        internal static void RegisterCustomCards(Harmony harmony)
        {
            // Register the sigils
            RegisterCustomAbilities(harmony);

            // Register the protected metacategories
            RegisterMetacategories();

            // Modify the squirrel
            CardManager.BaseGameCards.CardByName("Squirrel").SetSideDeck(CardTemple.Nature, 0);
            CardManager.BaseGameCards.CardByName("AquaSquirrel").SetPixelPortrait(AssetHelper.LoadTexture("pixelportrait_aquasquirrel"));
            CardManager.BaseGameCards.CardByName("PeltHare").SetPixelPortrait(AssetHelper.LoadTexture("pixelportrait_pelthare"));
            CardManager.BaseGameCards.CardByName("PeltWolf").SetPixelPortrait(AssetHelper.LoadTexture("pixelportrait_peltwolf"));

            // Update all of the old style side deck cards
            CardManager.ModifyCardList += delegate(List<CardInfo> cards)
            {
                foreach (CardInfo card in cards.Where(c => c.HasTrait(SideDeckManager.BACKWARDS_COMPATIBLE_SIDE_DECK_MARKER)))
                {
                    card.SetSideDeck(card.temple, 10);
                    card.traits.Remove(SideDeckManager.BACKWARDS_COMPATIBLE_SIDE_DECK_MARKER);
                }

                return cards;
            };

            // Create the squirrel
            CardManager.New(CustomCards.SideDecks.INF_Squirrel_Reach.ToString(),
                    "Squirrel",
                    0, 1,
                    "It's a squirrel that can block fliers")
                .SetSideDeck(CardTemple.Nature, 5)
                .SetPortrait(Resources.Load<Texture2D>("art/cards/portraits/portrait_squirrel"))
                .SetPixelPortrait(Resources.Load<Texture2D>("art/gbc/cards/pixelportraits/pixelportrait_squirrel"))
                .AddAbilities(Ability.Reach);

            // Create the Bee
            CardManager.New(CustomCards.SideDecks.INF_Bee_Drone.ToString(),
                    "Bee Drone", 1, 1, "For when you need just one point of damage")
                .SetSideDeck(CardTemple.Nature, 10)
                .SetPortrait(Resources.Load<Texture2D>("art/cards/portraits/portrait_bee"))
                .SetPixelPortrait(AssetHelper.LoadTexture("pixelportrait_bee"))
                .AddAbilities(Ability.Flying, Ability.Brittle)
                .AddTribes(Tribe.Insect);


            SpecialStatIcon antHealth = GuidManager.GetEnumValue<SpecialStatIcon>("julianperge.inscryption.cards.healthForAnts", "Ants (Health)");

            CardManager.New(CustomCards.SideDecks.INF_Ant_Worker.ToString(),
                    "Ant Drone", 0, 0, "It's not much, but it's an ant.")
                .SetSideDeck(CardTemple.Nature, 5)
                .SetPortrait(AssetHelper.LoadTexture("worker_ant"))
                .SetPixelPortrait(AssetHelper.LoadTexture("pixelportrait_ant_worker"))
                .AddTraits(Trait.Ant)
                .AddTribes(Tribe.Insect)
                .specialStatIcon = antHealth;

            foreach (StatIconInfo info in StatIconManager.AllStatIconInfos)
                SideDecksPlugin.Log.LogInfo($"Stat Icon: {info.rulebookName} ({info.iconType}), looking for {antHealth}");

            StatIconManager.AllStatIconInfos.First(si => si.iconType == antHealth).pixelIconGraphic = Sprite.Create(
                    Resources.Load<Texture2D>("art/gbc/cards/pixel_special_stat_icons"),
                    new Rect(0f, 27f, 16f, 8f),
                    new Vector2(0.5f, 0.5f)
                );

            // Create the Puppy
            CardManager.New(CustomCards.SideDecks.INF_Puppy.ToString(),
                    "Puppy", 0, 1, "This energetic little puppy will dig up a fresh bone every turn")
                .SetSideDeck(CardTemple.Nature, 10)
                .SetPortrait(AssetHelper.LoadTexture("digging_dog"))
                .SetPixelPortrait(AssetHelper.LoadTexture("pixel_digging_dog"))
                .AddAbilities(Ability.Strafe, Ability.BoneDigger)
                .AddTribes(Tribe.Canine);

            // Create the Squid Tail
            CardInfo tail = CardManager.New(CustomCards.SideDecks.INF_Spare_Tentacle.ToString() + "_Tail",
                    "Tentacle", 0, 2)
                .SetPortrait(AssetHelper.LoadTexture("squid_tail"))
                .AddAbilities(Gelatinous.AbilityID);
            tail.temple = CardTemple.Nature;
            tail.titleGraphic = Resources.Load<Texture2D>("art/cards/special/squid_title");

            // Create the Squid
            CardInfo tentacle = CardManager.New(CustomCards.SideDecks.INF_Spare_Tentacle.ToString(),
                    "Spare Tentacle", 0, 2, "I've never seen that [c:bR]thing[c:] before, but I can tell there are no bones in it.")
                .SetSideDeck(CardTemple.Nature, 10)
                .SetPortrait(AssetHelper.LoadTexture("squid_grunt"))
                .SetPixelPortrait(AssetHelper.LoadTexture("pixelportrait_squidgrunt"))
                .AddAbilities(Ability.TailOnHit, Gelatinous.AbilityID)
                .SetTail(CustomCards.SideDecks.INF_Spare_Tentacle.ToString() + "_Tail", AssetHelper.LoadTexture("squid_grunt_taillost"));
            tentacle.temple = CardTemple.Nature;
            tentacle.titleGraphic = Resources.Load<Texture2D>("art/cards/special/squid_title");

            // Create the Goat
            CardManager.New(CustomCards.SideDecks.INF_One_Eyed_Goat.ToString(),
                    "One-Eyed Goat", 0, 1, "This goat generates additional blood...for a price")
                .SetSideDeck(CardTemple.Nature, 10)
                .SetPortrait(AssetHelper.LoadTexture("portrait_goat_double"))
                .SetPixelPortrait(AssetHelper.LoadTexture("pixelportrait_one_eyed_goat"))
                .AddAbilities(DoubleTeeth.AbilityID, DoubleBlood.AbilityID)
                .AddTribes(Tribe.Hooved)
                .temple = CardTemple.Nature;

            CardManager.New(CustomCards.SideDecks.INF_Amalgam_Egg.ToString(),
                    "Amalgam Egg", 0, 1, "I didn't realize this thing came from eggs")
                .SetSideDeck(CardTemple.Nature, 10)
                .SetPortrait(AssetHelper.LoadTexture("egg"))
                .SetPixelPortrait(AssetHelper.LoadTexture("pixel_egg"))
                .temple = CardTemple.Nature;

            // Delay adding tribes in case another mod comes along and adds a tribe
            // This is just future proofing
            CardManager.ModifyCardList += delegate(List<CardInfo> cards)
            {
                CardInfo amalgamEgg = cards.CardByName(CustomCards.SideDecks.INF_Amalgam_Egg.ToString());
                if (amalgamEgg != null)
                    amalgamEgg.AddTribes(GuidManager.GetValues<Tribe>().ToArray());
                amalgamEgg.evolveParams = new() { evolution = cards.CardByName("Amalgam"), turnsToEvolve = 1 };
                amalgamEgg.iceCubeParams = new() { creatureWithin = cards.CardByName("Amalgam") };
                amalgamEgg.AddTraits(Trait.Ant);

                return cards;
            };

            CardManager.BaseGameCards.CardByName("EmptyVessel").SetSideDeck(CardTemple.Tech, 0);

            foreach (Ability ability in sideDeckableAbilities)
            {
                CardManager.New($"EmptyVessel{ability.ToString()}", "Empty Vessel", 0, 2)
                    .SetSideDeck(CardTemple.Tech, ability == Ability.GainBattery ? 20 : ability == Ability.Sharp || ability == Ability.Sentry ? 10 : 5)
                    .SetPortrait(Resources.Load<Texture2D>("art/cards/part 3 portraits/portrait_emptyvessel"))
                    .SetPixelPortrait(TextureHelper.GetImageAsTexture("pixel_emptyvessel.png", typeof(CustomCards).Assembly))
                    .SetCost(energyCost: 1)
                    .AddAbilities(ability);
            }

            CardManager.New($"EmptyVesselSubmerge", "Emptier Vessel", 0, 2)
                .SetPortrait(Resources.Load<Texture2D>("art/cards/part 3 portraits/portrait_emptyvessel"))
                .SetPixelPortrait(TextureHelper.GetImageAsTexture("pixel_emptyvessel.png", typeof(CustomCards).Assembly))
                .SetCost(energyCost: 1)
                .AddAbilities(Ability.Submerge);
        }
    }
}