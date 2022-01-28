using HarmonyLib;
using DiskCardGame;
using InscryptionAPI.Saves;
using System.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;
using Infiniscryption.Core.Helpers;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static class StarterDecks
    {
        public static Dictionary<Opponent.Type, List<StarterDeckInfo>> StarterDeckReference = new ();

        public static AscensionChooseStarterDeckScreen StarterDeckScreen { get; private set; }

        public static readonly StarterDeckInfo DUMMY_DECK = CreateStarterDeckInfo("NEVER_SEE_THIS", "starterdeck_icon_random", new string[] { "BatteryBot", "BatteryBot", "BatteryBot" });

        private static StarterDeckInfo CreateStarterDeckInfo(string title, Texture2D icon, string[] cards)
        {
            return new() {
                name=$"P03_{title}",
                title=title,
                iconSprite = Sprite.Create(icon, new Rect(0f, 0f, 35f, 44f), new Vector2(0.5f, 0.5f)),
                cards=cards.Select(CardLoader.GetCardByName).ToList()
            };
        }

        private static StarterDeckInfo CreateStarterDeckInfo(string title, string iconKey, string[] cards)
        {
            return CreateStarterDeckInfo(title, AssetHelper.LoadTexture(iconKey), cards);
        }

        static StarterDecks()
        {
            StarterDeckReference = new Dictionary<Opponent.Type, List<StarterDeckInfo>>();
            StarterDeckReference.Add(Opponent.Type.Default, new () {
                StarterDecksUtil.GetInfo("Vanilla"),
                StarterDecksUtil.GetInfo("Bones"),
                StarterDecksUtil.GetInfo("Ants"),
                StarterDecksUtil.GetInfo("MantisGod"),
                StarterDecksUtil.GetInfo("MooseBlood"),
                StarterDecksUtil.GetInfo("FreeReptiles"),
                StarterDecksUtil.GetInfo("Tentacles"),
            });

            StarterDeckReference.Add(Opponent.Type.P03Boss, new () {
                CreateStarterDeckInfo("Snipers", "starterdeck_icon_snipers", new string[] {"Sniper", "BustedPrinter", "SentryBot" }),
                CreateStarterDeckInfo("Random", "starterdeck_icon_random", new string[] {"Amoebot", "GiftBot", "GiftBot" }),
                CreateStarterDeckInfo("Shield", "starterdeck_icon_shield", new string[] {"GemShielder", "Shieldbot", "LatcherShield" }),
                CreateStarterDeckInfo("Energy", "starterdeck_icon_energy", new string[] {"CloserBot", "BatteryBot", "BatteryBot" }),//,
                CreateStarterDeckInfo("Conduit", "starterdeck_icon_conduit", new string[] {"CellTri", "CellBuff", "HealerConduit" }),//,
                CreateStarterDeckInfo("Nature", "starterdeck_icon_evolve", new string[] {"XformerGrizzlyBot", "XformerBatBot", "XformerPorcupineBot" }),//,
                CreateStarterDeckInfo("Gems", "starterdeck_icon_gems", new string[] {"SentinelBlue", "SentinelGreen", "SentinelOrange"}),
                CreateStarterDeckInfo("FullDraft", "starterdeck_icon_token", new string[] {CustomCards.UNC_TOKEN, CustomCards.DRAFT_TOKEN, CustomCards.DRAFT_TOKEN })//,
            });

            StarterDecksUtil.allData.AddRange(StarterDeckReference[Opponent.Type.P03Boss]);
        }

        private static int NumberOfConqueredP03Decks
        {
            get
            {
                int i = 0;
                foreach (var deck in StarterDeckReference[Opponent.Type.P03Boss])
                    if (AscensionSaveData.Data.conqueredStarterDecks.Contains(deck.name))
                        i++;
                return i;
            }
        }

        private static void SyncIconsToStarters(List<AscensionStarterDeckIcon> icons, List<StarterDeckInfo> decks)
        {
            for (int i = 0; i < icons.Count; i++)
                icons[i].AssignInfo(i >= decks.Count ? DUMMY_DECK : decks[i]);
        }

        [HarmonyPatch(typeof(AscensionUnlockSchedule), "StarterDeckIsUnlockedForLevel")]
        [HarmonyPrefix]
        public static bool P03StarterSchedule(ref bool __result, string id, int level)
        {
            if (id.StartsWith("P03"))
            {
                int numDecks = NumberOfConqueredP03Decks;

                // if (id.EndsWith("Conduit"))
                //     __result = numDecks >= 1;
                // else if (id.EndsWith("Nature"))
                //     __result = numDecks >= 2;
                // else if (id.EndsWith("Gems"))
                //     __result = numDecks >= 4;
                // else if (id.EndsWith("FullDraft"))
                //     __result = numDecks >= 7;
                // else
                __result = id != DUMMY_DECK.name;
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(AscensionChooseStarterDeckScreen), "OnEnable")]
        [HarmonyPrefix]
        public static void SetP03StarterDecks(ref AscensionChooseStarterDeckScreen __instance)
        {
            P03Plugin.Log.LogInfo($"Starter deck screen active - is P03? {ScreenManagement.ScreenState}");
            StarterDeckScreen = __instance; // Keep a reference to this for later

            SyncIconsToStarters(__instance.deckIcons, StarterDeckReference[ScreenManagement.ScreenState]);
        }

        [HarmonyPatch(typeof(AscensionStarterDeckIcon), "AssignInfo")]
        [HarmonyPrefix]
        public static void ForceAssignInfo(ref AscensionStarterDeckIcon __instance, StarterDeckInfo info)
        {
            __instance.starterDeckInfo = info;
            __instance.conqueredRenderer.enabled = false;
        }
    }
}