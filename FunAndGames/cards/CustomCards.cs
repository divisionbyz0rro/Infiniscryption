using DiskCardGame;
using Infiniscryption.Core.Helpers;
using InscryptionAPI.Ascension;
using InscryptionAPI.Card;
using UnityEngine;

namespace Infiniscryption.FunAndGames.Cards
{
    public static class CustomCards
    {
        public static readonly string KNIGHT = $"{GamesPlugin.CardPrefix}_Knight";
        public static readonly string PAWN = $"{GamesPlugin.CardPrefix}_Pawn";
        public static readonly string SQUIRREL = $"{GamesPlugin.CardPrefix}_SquirrelFriend";

        internal static void RegisterCards()
        {
            CardManager.New(GamesPlugin.CardPrefix, KNIGHT, "Knight", 2, 1)
                .SetPortrait(AssetHelper.LoadTexture("knight_portrait"), AssetHelper.LoadTexture("knight_emission"))
                .SetPixelPortrait(AssetHelper.LoadTexture("pixel_knight_portrait"))
                .AddAbilities(KnightStrike.ID)
                .SetCost(1)
                .SetDefaultPart1Card();

            CardManager.New(GamesPlugin.CardPrefix, PAWN, "Pawn", 1, 1)
                .SetPortrait(AssetHelper.LoadTexture("pawn_portrait_middle"), AssetHelper.LoadTexture("pawn_emission"))
                .SetPixelPortrait(AssetHelper.LoadTexture("pixel_pawn_portrait"))
                .AddAbilities(PawnStrike.ID)
                .AddSpecialAbilities(RenderOnSlotChanges.ID)
                .AddAppearances(PawnAppearance.ID)
                .SetCost(1)
                .SetDefaultPart1Card();

            CardManager.New(GamesPlugin.CardPrefix, SQUIRREL, "Squirrel Friend", 1, 1)
                .SetPortrait(AssetHelper.LoadTexture("pawn_portrait_middle"))
                .AddAbilities(SquirrelFriend.ID);

            StarterDeckManager.New(GamesPlugin.PluginGuid, "chess_set", AssetHelper.LoadTexture("starterdeck_icon_chess"), new string[] { PAWN, KNIGHT, SQUIRREL });
        }
    }
}
