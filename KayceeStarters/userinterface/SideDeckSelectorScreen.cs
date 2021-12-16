using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using UnityEngine;
using System.Linq;
using Infiniscryption.KayceeStarters.Cards;
using System;
using GBC;
using Infiniscryption.KayceeStarters.Patchers;

namespace Infiniscryption.KayceeStarters.UserInterface
{
    public class SideDeckSelectorScreen : AscensionCardsScreen
    {
        public static AscensionMenuScreens.Screen SIDE_DECK_SCREEN = (AscensionMenuScreens.Screen)5103;

        public static SideDeckSelectorScreen Instance { get; private set; }

        public static void Initialize()
        {
            if (Instance != null)
            {
                return;
            }

            // Create the new screen
            InfiniscryptionKayceeStartersPlugin.Log.LogInfo($"Starting to create side deck screen");

            InfiniscryptionKayceeStartersPlugin.Log.LogInfo($"Instantiating card unlocks screen");
            
            Traverse screensTraverse = Traverse.Create(AscensionMenuScreens.Instance);
            GameObject cardScreen = screensTraverse.Field("cardUnlockSummaryScreen").GetValue<GameObject>();
        
            //GameObject cardScreenPrefab = Resources.Load<GameObject>("prefabs/ui/ascension/ascensioncardssummaryscreen");
            GameObject screenObject = GameObject.Instantiate(cardScreen, cardScreen.transform.parent);

            InfiniscryptionKayceeStartersPlugin.Log.LogInfo($"Getting handle to old game logic");

            // Get rid of the existing game logic for this screen
            AscensionCardsSummaryScreen oldController = screenObject.GetComponent<AscensionCardsSummaryScreen>();
            Traverse oldTraverse = Traverse.Create(oldController);

            InfiniscryptionKayceeStartersPlugin.Log.LogInfo($"Adding new game logic and copying relevant fields");

            // And instead, add this game logic
            Instance = screenObject.AddComponent<SideDeckSelectorScreen>();
            Traverse newTraverse = Traverse.Create(Instance);
            Instance.cards = oldTraverse.Field("cards").GetValue<List<PixelSelectableCard>>();
            newTraverse.Field("descriptionLines").SetValue(oldTraverse.Field("descriptionLines").GetValue());
            
            InfiniscryptionKayceeStartersPlugin.Log.LogInfo($"Destroying old screen logic");
            Component.Destroy(oldController);

            InfiniscryptionKayceeStartersPlugin.Log.LogInfo($"Moving cards up");
            GameObject unlockPanel = screenObject.transform.Find("Unlocks").gameObject;
            unlockPanel.transform.localPosition = new Vector3(0f, 0.2f, 0f);

            InfiniscryptionKayceeStartersPlugin.Log.LogInfo($"Moving footer");
            GameObject cardTextDisplayer = screenObject.transform.Find("Footer/CardTextDisplayer").gameObject;
            cardTextDisplayer.transform.localPosition = new Vector3(2.38f, 0.27f, 0f);

            GameObject footerLowline = screenObject.transform.Find("Footer/PixelTextLine_DIV").gameObject;
            footerLowline.transform.localPosition = new Vector3(0f, 0.33f, 0f);

            InfiniscryptionKayceeStartersPlugin.Log.LogInfo($"Cloning Challenge Level");
            GameObject header = screenObject.transform.Find("Header").gameObject;
            SequentialPixelTextLines headerLines = header.GetComponent<SequentialPixelTextLines>();

            GameObject challengeScreen = screensTraverse.Field("selectChallengesScreen").GetValue<GameObject>();
            GameObject challengePseudoPrefab = challengeScreen.transform.Find("Header/ChallengeLevel").gameObject;
            GameObject pointsPseudoPrefab = challengeScreen.transform.Find("Header/ChallengePoints").gameObject;

            GameObject challengeLevel = GameObject.Instantiate(challengePseudoPrefab, header.transform);
            GameObject challengePoints = GameObject.Instantiate(pointsPseudoPrefab, header.transform);

            // Set these to the lines of the sequential pixel text lines
            List<PixelText> newLines = new List<PixelText>() {
                challengeLevel.GetComponent<PixelText>(),
                challengePoints.GetComponent<PixelText>()
            };
            Traverse.Create(headerLines).Field("lines").SetValue(newLines);

            // And add those lines to the new challenge level controller
            ChallengeLevelText challengeLevelController = screenObject.AddComponent<ChallengeLevelText>();
            Traverse.Create(challengeLevelController).Field("headerPointsLines").SetValue(headerLines);
            Instance.challengeLevelController = challengeLevelController;

            InfiniscryptionKayceeStartersPlugin.Log.LogInfo($"Setting footer text...");
            GameObject footer = screenObject.transform.Find("Footer").gameObject;

            List<Transform> footerLinePseudos = new List<Transform>();
            GameObject challengeFooter = challengeScreen.transform.Find("Footer").gameObject; 
            foreach (Transform child in challengeFooter.transform)
                if (child.name.ToLowerInvariant() == "pixeltextline")
                    footerLinePseudos.Add(child);
            footerLinePseudos.Sort((a, b) => a.localPosition.y < b.localPosition.y ? -1 : 1);

            List<GameObject> newFooterLines = footerLinePseudos.Select(t => GameObject.Instantiate(t.gameObject, footer.transform)).ToList();
            SequentialPixelTextLines footerLines = footer.AddComponent<SequentialPixelTextLines>();
            Traverse footerTraverse = Traverse.Create(footerLines);
            footerTraverse.Field("lines").SetValue(newFooterLines.Select(o => o.GetComponent<PixelText>()).ToList());
            footerTraverse.Field("linePrefix").SetValue(Traverse.Create(challengeFooter.GetComponent<SequentialPixelTextLines>()).Field("linePrefix").GetValue());
            Instance.footerLines = footerLines;

            InfiniscryptionKayceeStartersPlugin.Log.LogInfo($"Rerouting buttons");

            // Let's replace the behaviors of the left/right button
            GameObject leftButton = screenObject.transform.Find("Unlocks/ScreenAnchor/PageLeftButton").gameObject;
            GameObject rightButton = screenObject.transform.Find("Unlocks/ScreenAnchor/PageRightButton").gameObject;

            InfiniscryptionKayceeStartersPlugin.Log.LogInfo($"Got the buttons");

            leftButton.GetComponent<MainInputInteractable>().CursorSelectStarted = Instance.LeftButtonClicked;
            rightButton.GetComponent<MainInputInteractable>().CursorSelectStarted = Instance.RightButtonClicked;

            InfiniscryptionKayceeStartersPlugin.Log.LogInfo($"Rerouting the back button");
            GameObject backButton = screenObject.transform.Find("BackButton").gameObject;
            backButton.GetComponent<AscensionMenuBackButton>().screenToReturnTo = AscensionMenuScreens.Screen.SelectChallenges;

            InfiniscryptionKayceeStartersPlugin.Log.LogInfo($"Adding a continue button");
            GameObject continuePrefab = Resources.Load<GameObject>("prefabs/ui/ascension/ascensionmenucontinuebutton");
            GameObject continueButton = GameObject.Instantiate(continuePrefab, screenObject.transform);
            continueButton.transform.localPosition = new Vector3(2.08f, 1.15f, 0f);

            AscensionMenuInteractable continueComponent = continueButton.GetComponent<AscensionMenuInteractable>();
            continueComponent.CursorSelectStarted = (Action<MainInputInteractable>)Delegate.Combine(continueComponent.CursorSelectStarted, new Action<MainInputInteractable>(delegate(MainInputInteractable i)
				{
					Instance.OnContinueSelect();
				}));

            // And let's instantiate this with some cards
            InfiniscryptionKayceeStartersPlugin.Log.LogInfo($"Setting card infos");
            Instance.sideDeckCards = GetAllValidSideDeckCards().Select(CardLoader.GetCardByName).ToList();

            InfiniscryptionKayceeStartersPlugin.Log.LogInfo($"Adding three more cards");

            GameObject cardPrefab = Resources.Load<GameObject>("prefabs/gbccardbattle/pixelselectablecard");
            for (int i = 0; i < 3; i++)
            {
                GameObject newCard = GameObject.Instantiate(cardPrefab, Instance.cards[0].gameObject.transform.parent);
                PixelSelectableCard newPixelComponent = newCard.GetComponent<PixelSelectableCard>();
				newPixelComponent.CursorEntered = (Action<MainInputInteractable>)Delegate.Combine(newPixelComponent.CursorEntered, new Action<MainInputInteractable>(delegate(MainInputInteractable i)
				{
					Instance.DisplayCardDescription(newPixelComponent, false);
				}));
                Instance.cards.Add(newPixelComponent);

                // I have to manually connect the pixel border to the component for some reason
                GameObject pixelBorder = newCard.transform.Find("Base/PixelSnap/CardElements/PixelSelectionBorder").gameObject;
                pixelBorder.GetComponent<SpriteRenderer>().color = new Color(.619f, .149f, .188f); // Got this color off of the unityexplorer
                Traverse.Create(newPixelComponent).Field("pixelBorder").SetValue(pixelBorder);
            }

            InfiniscryptionKayceeStartersPlugin.Log.LogInfo($"Setting the click actions and removing lock texture");
            foreach (PixelSelectableCard card in Instance.cards)
            {
                card.CursorSelectStarted = (Action<MainInputInteractable>)Delegate.Combine(card.CursorSelectStarted, new Action<MainInputInteractable>(delegate(MainInputInteractable i)
				{
					Instance.CardClicked(card);
				}));

                Transform lockTexture = card.gameObject.transform.Find("Locked");
                if (lockTexture != null)
                    lockTexture.gameObject.SetActive(false);
            }

            InfiniscryptionKayceeStartersPlugin.Log.LogInfo($"Moving the scroll buttons");
            leftButton.transform.localPosition = leftButton.transform.localPosition - (Vector3)(1.5f * BETWEEN_CARD_OFFSET);
            rightButton.transform.localPosition = rightButton.transform.localPosition + (Vector3)(1.5f * BETWEEN_CARD_OFFSET);

            InfiniscryptionKayceeStartersPlugin.Log.LogInfo($"Will disable the buttons if there are 7 or fewer sidedeck cards");
            if (Instance.sideDeckCards.Count <= 7)
            {
                leftButton.SetActive(false);
                rightButton.SetActive(false);
            }

            // Create a single pixel selection border that will be used to surround the selected card
            GameObject selectedBorderPrefab = Resources.Load<GameObject>("prefabs/gbcui/pixelselectionborder");
            GameObject selectedBorder = GameObject.Instantiate(selectedBorderPrefab, Instance.cards[0].gameObject.transform.parent);
            selectedBorder.GetComponent<SpriteRenderer>().color = GameColors.Instance.brightNearWhite; // Got this color off of the unityexplorer
            Instance.selectedBorder = selectedBorder;

            // Start the new page
            Instance.ShowPage();
            Instance.CardClicked(Instance.cards[0]);

            InfiniscryptionKayceeStartersPlugin.Log.LogInfo($"Changing title");
            GameObject textHeader = screenObject.transform.Find("Header/Mid").gameObject;
            textHeader.transform.localPosition = new Vector3(0f, -0.575f, 0f);
            textHeader.transform.Find("Text").gameObject.GetComponent<UnityEngine.UI.Text>().text = "Choose a side deck";

            InfiniscryptionKayceeStartersPlugin.Log.LogInfo($"We are done!");
        }

        [HarmonyPatch(typeof(AscensionChallengeScreen), "OnContinueCursorEnter")]
        [HarmonyPrefix]
        public static bool ChangeHoverTextForChallengeConfirm(ref AscensionChallengeScreen __instance)
        {
            string line = Localization.Translate("CHOOSE SIDE DECK");
            AscensionChallengeDisplayer disp = Traverse.Create(__instance).Field("challengeDisplayer").GetValue<AscensionChallengeDisplayer>();
	        disp.DisplayText("", line, "", false);
            return false;
        }

        [HarmonyPatch(typeof(AscensionChallengeScreen), "OnContinuePressed")]
        [HarmonyPrefix]
        public static bool TransitionToSideDeckScreen(ref AscensionChallengeScreen __instance)
        {
            AscensionMenuScreens.Instance.SwitchToScreen(SIDE_DECK_SCREEN);
            return false;
        }

        [HarmonyPatch(typeof(AscensionMenuScreens), "DeactivateAllScreens")]
        [HarmonyPostfix]
        public static void DeactivateSideDeckScreen()
        {
            if (SideDeckSelectorScreen.Instance == null)
                SideDeckSelectorScreen.Initialize(); // Also initializes the screen we care about
            
            SideDeckSelectorScreen.Instance.gameObject.SetActive(false);
        }

        [HarmonyPatch(typeof(AscensionSaveData), "EndRun")]
        [HarmonyPostfix]
        public static void ResetSideDeckSelection()
        {
            if (SideDeckSelectorScreen.Instance != null)
                SideDeckSelectorScreen.Instance.resetSelection = true;
        }

        private static List<string> GetAllValidSideDeckCards()
        {
            return CardLoader.AllData.Where(
                    card => card.traits.Any(tr => (int)tr == (int)CustomCards.SIDE_DECK_MARKER)
                ).Select(card => card.name).ToList();
        }

        public void LeftButtonClicked(MainInputInteractable button)
        {
            if (scrollIndex > 0)
            {
                scrollIndex -= 1;
                ShowPage();
            }
        }

        public void RightButtonClicked(MainInputInteractable button)
        {
            if ((scrollIndex + 1) * this.cards.Count < sideDeckCards.Count)
            {
                scrollIndex += 1;
                ShowPage();
            }
        }

        private void UpdateFooterText()
		{
            CardInfo info = CardLoader.GetCardByName(SideDeckPatcher.SelectedSideDeck);
            string cardName = Localization.ToUpper(info.DisplayedNameLocalized);

            string lineOne = string.Format(Localization.Translate("{0} SELECTED"), cardName);

            string lineTwo;
			if (SideDeckPatcher.SelectedSideDeck == CustomCards.SideDecks.Squirrel.ToString())
            {
				lineTwo = string.Format(Localization.Translate("{0} Challenge Points"), 0);
			}
			else
			{
				lineTwo = string.Format(Localization.Translate("{0} Challenge Points Subtracted"), 5);
			}

			int challengeLevel = AscensionSaveData.Data.challengeLevel;
			int activeChallengePoints = AscensionSaveData.Data.GetActiveChallengePoints();
			string lineThree;
			if (activeChallengePoints > challengeLevel * 10)
			{
				lineThree = string.Format(Localization.Translate("WARNING(!) Lvl Reqs EXCEEDED"), Array.Empty<object>());
			}
			else
			{
				if (activeChallengePoints == challengeLevel * 10)
				{
					lineThree = string.Format(Localization.Translate("Lvl Reqs Met"), Array.Empty<object>());
				}
				else
				{
					lineThree = string.Format(Localization.Translate("Lvl Reqs NOT MET"), Array.Empty<object>());
				}
			}
			this.footerLines.ShowText(0.1f, new string[]
			{
				lineOne,
				lineTwo,
				lineThree
			}, false);
		}

        private void VisualSelectionSync()
        {
            if (challengeLevelController != null && this.gameObject.activeSelf)
            {
                challengeLevelController.UpdateText();
                UpdateFooterText();
                ShowBorder();
            }
        }

        public void CardClicked(PixelSelectableCard card)
        {
            InfiniscryptionKayceeStartersPlugin.Log.LogInfo("Player selected sidedeck card");
            if (card != null && card.Info != null)
            {
                InfiniscryptionKayceeStartersPlugin.Log.LogInfo($"Setting selection to {card.Info.name}");
                SideDeckPatcher.SelectedSideDeck = card.Info.name;

                VisualSelectionSync();
            }
        }

        public void OnContinueSelect()
        {
            if (!AscensionSaveData.Data.ChallengeLevelIsMet() && AscensionSaveData.Data.challengeLevel <= 12)
            {
                AscensionMenuScreens.Instance.SwitchToScreen(AscensionMenuScreens.Screen.SelectChallengesConfirm);
            }
            else
            {
                AscensionMenuScreens.Instance.TransitionToGame(true);
            }
        }

        public List<CardInfo> sideDeckCards = new List<CardInfo>();

        private int scrollIndex = 0;

        private ChallengeLevelText challengeLevelController;

        private SequentialPixelTextLines footerLines;

        private GameObject selectedBorder;

        private static Vector2 FIRST_CARD_OFFSET = new Vector2(-1.5f, 0f);

        private static Vector2 BETWEEN_CARD_OFFSET = new Vector2(0.5f, 0f);

        public bool resetSelection = false;

        private void ShowBorder()
        {
            foreach (PixelSelectableCard card in this.cards)
            {
                if (card.Info.name == SideDeckPatcher.SelectedSideDeck)
                {
                    this.selectedBorder.SetActive(true);
                    this.selectedBorder.transform.localPosition = card.transform.localPosition;
                    return;
                }
            }

            this.selectedBorder.SetActive(false);
        }

        public void ShowPage()
        {
            InfiniscryptionKayceeStartersPlugin.Log.LogInfo($"Side deck screen: setting page {scrollIndex}. I have {this.cards.Count} card objects to play with");

            int startIdx = this.cards.Count * scrollIndex;
            int numToShow = Math.Min(this.cards.Count, sideDeckCards.Count - startIdx);

            foreach (PixelSelectableCard card in this.cards)
                card.gameObject.SetActive(false);

            float gaps = ((float)(this.cards.Count - numToShow)) / 2f;
            Vector2 startPos = FIRST_CARD_OFFSET + BETWEEN_CARD_OFFSET * gaps;

            for (int i = 0; i < numToShow; i++)
            {
                this.cards[i].SetInfo(this.sideDeckCards[startIdx + i]);
                this.cards[i].gameObject.transform.localPosition = startPos + (float)i * BETWEEN_CARD_OFFSET;
                this.cards[i].gameObject.SetActive(true);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (resetSelection)
            {
                CardClicked(this.cards[0]);
                resetSelection = false;
            }

            VisualSelectionSync();
        }
    }
}