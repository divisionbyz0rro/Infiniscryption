using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using UnityEngine;
using System.Linq;
using System;
using GBC;
using InscryptionAPI.Ascension;
using Infiniscryption.SideDecks.Patchers;

namespace Infiniscryption.SideDecks.UserInterface
{
    [AscensionScreenSort(AscensionScreenSort.Direction.PrefersEnd)]
    public class SideDeckSelectorScreen : AscensionRunSetupScreenBase
    {
        public override string headerText => "Select a Side Deck";
        public override bool showCardDisplayer => true;
        public override bool showCardPanel => true;

        private List<CardInfo> sideDeckCards;

        public static SideDeckSelectorScreen Instance;

        private static bool secondInitialized = false;

        public override void InitializeScreen(GameObject partialScreen)
        {
            // Create a single pixel selection border that will be used to surround the selected card
            GameObject selectedBorderPrefab = Resources.Load<GameObject>("prefabs/gbcui/pixelselectionborder");
            GameObject selectedBorder = GameObject.Instantiate(selectedBorderPrefab, this.cards[0].gameObject.transform.parent);
            selectedBorder.GetComponent<SpriteRenderer>().color = GameColors.Instance.brightNearWhite; // Got this color off of the unityexplorer
            selectedBorder.AddComponent<PixelSnapElement>();
            this.selectedBorder = selectedBorder;

            // Save the instance
            Instance = this;
        }

        public const int SIDE_DECK_SIZE = 10;

        [HarmonyPatch(typeof(AscensionSaveData), "EndRun")]
        [HarmonyPostfix]
        public static void ResetSideDeckSelection()
        {
            if (SideDeckSelectorScreen.Instance != null)
            {
                SideDeckManager.SelectedSideDeck = "Squirrel";
                SideDeckSelectorScreen.Instance.resetSelection = true;
            }
        }

        [HarmonyPatch(typeof(AscensionSaveData), "GetActiveChallengePoints")]
        [HarmonyPostfix]
        public static void ReduceChallengeIfCustomSideDeckSelected(ref int __result)
        {
            if (SideDeckSelectorScreen.Instance != null)
                __result += SideDeckSelectorScreen.Instance.SideDeckPoints;
        }

        public override void LeftButtonClicked(MainInputInteractable button)
        {
            if (scrollIndex > 0)
            {
                scrollIndex -= 1;
                ShowPage();
            }
        }

        public override void RightButtonClicked(MainInputInteractable button)
        {
            if ((scrollIndex + 1) * this.cards.Count < this.sideDeckCards.Count)
            {
                scrollIndex += 1;
                ShowPage();
            }
        }

        public int SideDeckPoints { get; private set; }

        private void VisualUpdate(bool immediate=false)
        {
            CardInfo selectedCard = CardLoader.GetCardByName(SideDeckManager.SelectedSideDeck);

            string message = String.Format(Localization.Translate("{0} SELECTED"), Localization.ToUpper(selectedCard.DisplayedNameLocalized));
            SideDeckPoints = selectedCard.name == sideDeckCards[0].name ? 0 : -10;
            this.DisplayChallengeInfo(message, SideDeckPoints, immediate);

            this.challengeHeaderDisplay.UpdateText();

            // This bit sorts out the border    
            foreach (PixelSelectableCard card in this.cards)
            {
                if (card.Info.name == SideDeckManager.SelectedSideDeck)
                {
                    this.selectedBorder.SetActive(true);
                    this.selectedBorder.transform.SetParent(card.transform.Find("Base/PixelSnap"), false);
                    return;
                }
            }

            this.selectedBorder.SetActive(false);
        }

        public override void CardClicked(PixelSelectableCard card)
        {
            SideDecksPlugin.Log.LogInfo("Player selected sidedeck card");
            if (card != null && card.Info != null)
            {
                SideDecksPlugin.Log.LogInfo($"Setting selection to {card.Info.name}");
                SideDeckManager.SelectedSideDeck = card.Info.name;

                VisualUpdate();
            }
        }

        private bool inHardMode = false;

        private int scrollIndex = 0;

        private GameObject selectedBorder;

        public bool resetSelection = true;

        public void ShowPage()
        {
            SideDecksPlugin.Log.LogInfo($"Side deck screen: setting page {scrollIndex}. I have {this.cards.Count} card objects to play with");

            int startIdx = this.cards.Count * scrollIndex;
            int numToShow = Math.Min(this.cards.Count, sideDeckCards.Count - startIdx);
            this.ShowCards(sideDeckCards.GetRange(startIdx, numToShow));
        }

        public void InitializeCardSelection()
        {
            SideDecksPlugin.Log.LogInfo($"Setting card infos");
            this.sideDeckCards = SideDeckManager.GetAllValidSideDeckCards().Select(CardLoader.GetCardByName).ToList();

            // Hide the left and right buttons if the number of available side deck cards is <= the number of card panels
            this.leftButton.gameObject.SetActive(this.cards.Count < this.sideDeckCards.Count);
            this.rightButton.gameObject.SetActive(this.cards.Count < this.sideDeckCards.Count);
        }

        public override void OnEnable()
        {
            base.OnEnable();

            if (resetSelection || inHardMode != AscensionSaveData.Data.ChallengeIsActive(AscensionChallenge.SubmergeSquirrels))
            {
                inHardMode = AscensionSaveData.Data.ChallengeIsActive(AscensionChallenge.SubmergeSquirrels);
                resetSelection = false;
                scrollIndex = 0;
                InitializeCardSelection();
                ShowPage();
                CardClicked(this.cards[0]);
            }

            VisualUpdate(true);
        }
    }
}