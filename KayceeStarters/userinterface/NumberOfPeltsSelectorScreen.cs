using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using UnityEngine;
using System.Linq;
using Infiniscryption.KayceeStarters.Cards;
using System;
using GBC;
using InscryptionAPI.AscensionScreens;
using InscryptionAPI.Saves;

namespace Infiniscryption.KayceeStarters.UserInterface
{
    [AscensionScreenSort(AscensionScreenSort.Direction.NoPreference)]
    public class NumberOfPeltsSelectionScreen : AscensionRunSetupScreenBase
    {
        public const string DRAFT_MATERIAL_GUID = "zorro.inscryption.infiniscryption.draftmaterial";

        public override string headerText => "Choose Starting Cards";
        public override bool showCardDisplayer => true;
        public override bool showCardPanel => true;

        public static int HARE_COST = 5;
        public static int WOLF_COST = 10;

        public static NumberOfPeltsSelectionScreen Instance;

        public override void InitializeScreen(GameObject partialScreen)
        {
            Instance = this;
        }

        [HarmonyPatch(typeof(AscensionSaveData), "EndRun")]
        [HarmonyPostfix]
        public static void ResetSideDeckSelection()
        {
            if (NumberOfPeltsSelectionScreen.Instance != null)
                NumberOfPeltsSelectionScreen.Instance.resetSelection = true;
        }

        [HarmonyPatch(typeof(AscensionSaveData), "NewRun")]
        [HarmonyPostfix]
        public static void ResetDeck(ref AscensionSaveData __instance)
        {
            if (NumberOfPeltsSelectionScreen.Instance != null)
            {
                __instance.currentRun.playerDeck = new DeckInfo();
                foreach (CardInfo card in NumberOfPeltsSelectionScreen.Instance.currentDeck)
                {
                    __instance.currentRun.playerDeck.AddCard(card);
                }
            }
        }

        [HarmonyPatch(typeof(MapGenerator), "ForceFirstNodeTraderForAscension")]
        [HarmonyPostfix]
        public static void OverrideTraderBehavior(ref bool __result, int rowIndex)
        {
            __result = SaveFile.IsAscension && rowIndex == 1 && RunState.Run.regionTier == 0 && 
                       AscensionSaveData.Data.currentRun.playerDeck.Cards.Where(c => c.name.ToLowerInvariant().Contains("pelt")).Count() > 0;
        }

        [HarmonyPatch(typeof(AscensionSaveData), "GetActiveChallengePoints")]
        [HarmonyPostfix]
        public static void ReduceChallengeIfCustomSideDeckSelected(ref int __result)
        {
            if (NumberOfPeltsSelectionScreen.Instance != null)
                __result += NumberOfPeltsSelectionScreen.Instance.deckScore;
        }

        public List<CardInfo> defaultDeck;

        public List<CardInfo> currentDeck;

        private static readonly List<CardInfo> defaultDraftMaterial = new() {
            CardLoader.GetCardByName("WolfPelt"),
            CardLoader.GetCardByName("HarePelt"),
            CardLoader.GetCardByName("HarePelt"),
            CardLoader.GetCardByName("HarePelt"),
            CardLoader.GetCardByName("HarePelt"),
        };

        private static List<CardInfo> GetDraftMaterialForDeck(string deckname)
        {
            string saveFileMaterial = ModdedSaveManager.SaveData.GetValue(DRAFT_MATERIAL_GUID, deckname);
            if (string.IsNullOrEmpty(saveFileMaterial))
                return defaultDraftMaterial;
            
            string[] deckList = saveFileMaterial.Split(',');
            if (deckList.Length != 5)
                return defaultDraftMaterial;

            return deckList.Select(CardLoader.GetCardByName).ToList();
        }

        private static List<CardInfo> GetDefaultDeck()
        {
            GameObject screen = Traverse.Create(AscensionMenuScreens.Instance).Field("starterDeckSelectScreen").GetValue<GameObject>();
            StarterDeckInfo starterDeck = screen.GetComponent<AscensionChooseStarterDeckScreen>().SelectedInfo;
            List<CardInfo> retval = starterDeck.cards
                   .AddItem(CardLoader.GetCardByName("Opossum"))
                   .AddItem(CardLoader.GetCardByName("RingWorm"))
                   .ToList();

            retval.Sort((a, b) => b.PowerLevel - a.PowerLevel);
            return retval;
        }

        private int numberOfPelts = 2;

        public override void LeftButtonClicked(MainInputInteractable button)
        {
            if (numberOfPelts < defaultDeck.Count)
            {
                numberOfPelts += 1;
                ShowPage();
            }
        }

        public override void RightButtonClicked(MainInputInteractable button)
        {
            if (numberOfPelts > 0)
            {
                numberOfPelts -= 1;
                ShowPage();
            }
        }

        public bool resetSelection = true;

        public int deckScore { get; private set; }

        public void RecalculateChallengePoints(bool immediate=false)
        {
            // Any movement costs 5
            // Going to all pelts costs an additional 10
            deckScore = -2 * HARE_COST;
            deckScore += this.currentDeck.Select(card => card.name == "PeltWolf" ? WOLF_COST : card.name == "PeltHare" ? HARE_COST : 0).Sum();
            deckScore = -Math.Abs(deckScore);
            this.DisplayChallengeInfo("DECK ALTERED", deckScore, immediate:immediate);
            this.challengeHeaderDisplay.UpdateText();
        }

        public void ShowPage()
        {
            InfiniscryptionKayceeStartersPlugin.Log.LogInfo($"Pelts screen: setting pelts to {numberOfPelts}");

            this.currentDeck = new List<CardInfo>();
            if (numberOfPelts < this.defaultDeck.Count)
                this.currentDeck.AddRange(this.defaultDeck.GetRange(0, this.defaultDeck.Count - numberOfPelts));

            if (numberOfPelts == this.defaultDeck.Count)
                this.currentDeck.Add(CardLoader.GetCardByName("PeltWolf"));

            for (int i = 0; i < Math.Min(numberOfPelts, this.defaultDeck.Count - 1); i++)
                this.currentDeck.Add(CardLoader.GetCardByName("PeltHare"));

            this.ShowCards(this.currentDeck);

            RecalculateChallengePoints();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            this.defaultDeck = GetDefaultDeck();

            if (resetSelection)
            {
                numberOfPelts = 2;
                ShowPage();
                resetSelection = false;
            }

            RecalculateChallengePoints(immediate:true);
        }
    }
}