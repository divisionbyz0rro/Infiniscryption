using DiskCardGame;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using Infiniscryption.Core.Helpers;
using InscryptionAPI.Ascension;
using InscryptionAPI.Card;

namespace Infiniscryption.Curses.Patchers
{
    public static class StarterDecks
    {
        public static void Register(Harmony harmony)
        {
            CardManager.BaseGameCards.CardByName("PeltWolf").SetPixelPortrait(AssetHelper.LoadTexture("pixelportrait_peltwolf"));
            CardManager.BaseGameCards.CardByName("PeltHare").SetPixelPortrait(AssetHelper.LoadTexture("pixelportrait_pelthare"));

            // Create the pelts starter deck
            StarterDeckManager.New(
                CursePlugin.PluginGuid,
                "Pelts",
                AssetHelper.LoadTexture("starterdeck_icon_pelts"),
                new string[] { "PeltWolf", "PeltHare", "PeltHare" }
            );
        }
    }
}