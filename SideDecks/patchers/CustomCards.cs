using UnityEngine;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.Core.Helpers;
using InscryptionAPI.Guid;
using Infiniscryption.SideDecks.Sigils;
using InscryptionAPI.Card;
using Infiniscryption.SideDecks;

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
            INF_One_Eyed_Goat = 6
        }

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
            CardManager.AllCards.CardByName("Squirrel").metaCategories.Add(SideDeckManager.SIDE_DECK);
            CardManager.AllCards.CardByName("AquaSquirrel").SetPixelPortrait(AssetHelper.LoadTexture("pixelportrait_aquasquirrel"));
            CardManager.AllCards.CardByName("PeltHare").SetPixelPortrait(AssetHelper.LoadTexture("pixelportrait_pelthare"));
            CardManager.AllCards.CardByName("PeltWolf").SetPixelPortrait(AssetHelper.LoadTexture("pixelportrait_peltwolf"));
            AbilityManager.AllAbilityInfos.AbilityByID(Ability.TailOnHit).SetPixelAbilityIcon(AssetHelper.LoadTexture("pixelability_tailonhit"));

            // Create the squirrel
            CardManager.New(CustomCards.SideDecks.INF_Squirrel_Reach.ToString(),
                    "Squirrel",
                    0, 1,
                    "It's a squirrel that can block fliers")
                .SetSideDeck(CardTemple.Nature)
                .SetPortrait(Resources.Load<Texture2D>("art/cards/portraits/portrait_squirrel"))
                .SetPixelPortrait(Resources.Load<Texture2D>("art/gbc/cards/pixelportraits/pixelportrait_squirrel"))
                .AddAbilities(Ability.Reach);

            // Create the Bee
            CardManager.New(CustomCards.SideDecks.INF_Bee_Drone.ToString(),
                    "Bee Drone", 1, 1, "For when you need just one point of damage")
                .SetSideDeck(CardTemple.Nature)
                .SetPortrait(Resources.Load<Texture2D>("art/cards/portraits/portrait_bee"))
                .SetPixelPortrait(AssetHelper.LoadTexture("pixelportrait_bee"))
                .AddAbilities(Ability.Flying, Ability.Brittle)
                .AddTribes(Tribe.Insect);

            //Create the Ant
            // var antHealthAbility = HealthForAnts.HarmonyInit.antHealthSpecialAbility;
            // antHealthAbility.statIconInfo.pixelIconGraphic = Sprite.Create(
            //     Resources.Load<Texture2D>("art/gbc/cards/pixel_special_stat_icons"),
            //     new Rect(0f, 27f, 16f, 8f),
            //     new Vector2(0.5f, 0.5f)
            // );
            // antHealthAbility.statIconInfo.gbcDescription = "The health of [creature] is equal to the number of Ants that the owner has on their side of the table.";
            // NewCard.Add(
            //     CustomCards.SideDecks.INF_Ant_Worker.ToString(),
            //     "Ant Drone",
            //     0, 0,
            //     new List<CardMetaCategory>() { },
            //     CardComplexity.Vanilla,
            //     CardTemple.Nature,
            //     "It's not much, but it's an ant.",
            //     defaultTex: AssetHelper.LoadTexture("worker_ant"),
            //     pixelTex: AssetHelper.LoadTexture("pixelportrait_ant_worker"),
            //     tribes: new List<Tribe>() { Tribe.Insect },
            //     traits: new List<Trait>() { Trait.Ant, BACKWARDS_COMPATIBLE_SIDE_DECK_MARKER },
            //     specialStatIcon: antHealthAbility.statIconInfo.iconType,
            //     specialAbilitiesIdsParam: new List<SpecialAbilityIdentifier>() { antHealthAbility.id }
            // );

            // Create the Puppy
            CardManager.New(CustomCards.SideDecks.INF_Puppy.ToString(),
                    "Puppy", 0, 1, "This energetic little puppy will dig up a fresh bone every turn")
                .SetSideDeck(CardTemple.Nature)
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
                .SetSideDeck(CardTemple.Nature)
                .SetPortrait(AssetHelper.LoadTexture("squid_grunt"))
                .SetPixelPortrait(AssetHelper.LoadTexture("pixelportrait_squidgrunt"))
                .AddAbilities(Ability.TailOnHit, Gelatinous.AbilityID)
                .SetTail(CustomCards.SideDecks.INF_Spare_Tentacle.ToString() + "_Tail", AssetHelper.LoadTexture("squid_grunt_taillost"));
            tentacle.temple = CardTemple.Nature;
            tentacle.titleGraphic = Resources.Load<Texture2D>("art/cards/special/squid_title");

            // Create the Goat
            CardManager.New(CustomCards.SideDecks.INF_One_Eyed_Goat.ToString(),
                    "One-Eyed Goat", 0, 1, "This goat generates additional blood...for a price")
                .SetSideDeck(CardTemple.Nature)
                .SetPortrait(AssetHelper.LoadTexture("portrait_goat_double"))
                .SetPixelPortrait(AssetHelper.LoadTexture("pixelportrait_one_eyed_goat"))
                .AddAbilities(DoubleTeeth.AbilityID, DoubleBlood.AbilityID)
                .temple = CardTemple.Nature;
        }
    }
}