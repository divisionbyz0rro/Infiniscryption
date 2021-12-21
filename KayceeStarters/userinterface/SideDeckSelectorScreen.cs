using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using UnityEngine;
using System.Linq;
using Infiniscryption.KayceeStarters.Cards;
using System;
using GBC;
using Infiniscryption.KayceeStarters.Patchers;
using InscryptionAPI.AscensionScreens;

namespace Infiniscryption.KayceeStarters.UserInterface
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
            this.selectedBorder = selectedBorder;

            // Save the instance
            Instance = this;
        }

        [HarmonyPatch(typeof(AscensionSaveData), "EndRun")]
        [HarmonyPostfix]
        public static void ResetSideDeckSelection()
        {
            if (SideDeckSelectorScreen.Instance != null)
            {
                KayceesDeckboxPatcher.SelectedSideDeck = "Squirrel";
                SideDeckSelectorScreen.Instance.resetSelection = true;
            }
        }

        private static List<string> GetAllValidSideDeckCards()
        {
            return CardLoader.AllData.Where(
                    card => card.traits.Any(tr => (int)tr == (int)CustomCards.SIDE_DECK_MARKER)
                ).Select(card => card.name).ToList();
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
            List<CardInfo> cardsToShow = inHardMode ? hardModeSideDeckCards : sideDeckCards;
            if ((scrollIndex + 1) * this.cards.Count < cardsToShow.Count)
            {
                scrollIndex += 1;
                ShowPage();
            }
        }

        private void VisualUpdate(bool immediate=false)
        {
            if (this.gameObject.activeSelf)
            {
                CardInfo selectedCard = CardLoader.GetCardByName(KayceesDeckboxPatcher.SelectedSideDeck);

                string message = String.Format(Localization.Translate("{0} SELECTED"), Localization.ToUpper(selectedCard.DisplayedNameLocalized));
                int points = selectedCard.name.ToLowerInvariant() == "Squirrel" ? 0 : -5;
                this.DisplayChallengeInfo(message, points, immediate);

                this.challengeHeaderDisplay.UpdateText();

                // This bit sorts out the border    
                foreach (PixelSelectableCard card in this.cards)
                {
                    if (card.Info.name == KayceesDeckboxPatcher.SelectedSideDeck)
                    {
                        this.selectedBorder.SetActive(true);
                        this.selectedBorder.transform.localPosition = card.transform.localPosition;
                        return;
                    }
                }

                this.selectedBorder.SetActive(false);
            }
        }

        public override void CardClicked(PixelSelectableCard card)
        {
            InfiniscryptionKayceeStartersPlugin.Log.LogInfo("Player selected sidedeck card");
            if (card != null && card.Info != null)
            {
                InfiniscryptionKayceeStartersPlugin.Log.LogInfo($"Setting selection to {card.Info.name}");
                KayceesDeckboxPatcher.SelectedSideDeck = card.Info.name;

                VisualUpdate();
            }
        }

        public List<CardInfo> hardModeSideDeckCards; 

        private bool inHardMode = false;

        private int scrollIndex = 0;

        private GameObject selectedBorder;

        public bool resetSelection = false;

        public void ShowPage()
        {
            InfiniscryptionKayceeStartersPlugin.Log.LogInfo($"Side deck screen: setting page {scrollIndex}. I have {this.cards.Count} card objects to play with");

            List<CardInfo> cardsToShow = inHardMode ? hardModeSideDeckCards : sideDeckCards;

            int startIdx = this.cards.Count * scrollIndex;
            int numToShow = Math.Min(this.cards.Count, cardsToShow.Count - startIdx);
            this.ShowCards(cardsToShow.GetRange(startIdx, numToShow));
        }

        public void InitializeCardSelection()
        {
            InfiniscryptionKayceeStartersPlugin.Log.LogInfo($"Setting card infos");
            this.sideDeckCards = GetAllValidSideDeckCards().Select(CardLoader.GetCardByName).ToList();
            this.hardModeSideDeckCards = new List<CardInfo>() { CardLoader.GetCardByName("AquaSquirrel") };

            // Hide the left and right buttons if the number of available side deck cards is <= the number of card panels
            this.leftButton.gameObject.SetActive(this.cards.Count < this.sideDeckCards.Count);
            this.rightButton.gameObject.SetActive(this.cards.Count < this.sideDeckCards.Count);
        }

        protected override void OnEnable()
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