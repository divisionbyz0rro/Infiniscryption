using System;
using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using Pixelplacement;
using UnityEngine;
using HarmonyLib;
using Infiniscryption.SideDecks.Patchers;
using System.Linq;
using InscryptionAPI.Encounters;
using InscryptionAPI.Helpers;
using InscryptionAPI.Ascension;
using Infiniscryption.Core.Helpers;

namespace Infiniscryption.SideDecks.Sequences
{
    public class SideDeckSelectionSequencer : CardChoicesSequencer, ICustomNodeSequence
	{        
        public static AscensionChallenge ChallengeID { get; private set; }

        public class SideDeckSelectionNode : CustomNodeData
        {
            public override void Initialize()
            {
                this.AddGenerationPrerequisite(() => !AscensionSaveData.Data.ChallengeIsActive(AscensionChallenge.SubmergeSquirrels));
                this.GenerationPrerequisiteConditions.Add(new NodeData.PreviousNodesContent(typeof(SideDeckSelectionNode), false));
                this.AddGenerationPrerequisite(() => RunState.Run.regionTier == 1 || RunState.Run.regionTier == 2);
                this.AddGenerationPrerequisite(() => AscensionSaveData.Data.ChallengeIsActive(ChallengeID));
            }
        }

        public static void Register()
        {
            NodeManager.Add<SideDeckSelectionSequencer, SideDeckSelectionNode>(
                new Texture2D[] {
                    TextureHelper.GetImageAsTexture("animated_sidedeck_1.png", typeof(SideDeckSelectionSequencer).Assembly),
                    TextureHelper.GetImageAsTexture("animated_sidedeck_2.png", typeof(SideDeckSelectionSequencer).Assembly),
                    TextureHelper.GetImageAsTexture("animated_sidedeck_3.png", typeof(SideDeckSelectionSequencer).Assembly),
                    TextureHelper.GetImageAsTexture("animated_sidedeck_4.png", typeof(SideDeckSelectionSequencer).Assembly)
                },
                NodeManager.NodePosition.MapStart
            );

            ChallengeID = ChallengeManager.Add
            (
                SideDecksPlugin.PluginGuid,
                "Side Deck Second Chance",
                "You are given the option to change your side deck on maps two and three",
                -10,
                AssetHelper.LoadTexture("assist_side_deck_toggle"),
                ChallengeManager.HAPPY_ACTIVATED_SPRITE
            ).Challenge.challengeType;
        }

        private static Traverse _parentContainer;
        private static T GetCopiedField<T>(string fieldName) where T : class
        {
            if (_parentContainer == null)
                _parentContainer = Traverse.Create(Traverse.Create(SpecialNodeHandler.Instance).Field("cardChoiceSequencer").GetValue() as CardSingleChoicesSequencer);

            return _parentContainer.Field(fieldName).GetValue() as T;
        }

        private void Initialize()
        {
            // Does some basic setup

            if (base.selectableCardPrefab == null)
                base.selectableCardPrefab = GetCopiedField<GameObject>("selectableCardPrefab");

            this.basePosition = base.transform.position;
        }

        private string selectedCardName = null;
        private Vector3 basePosition;

		public override IEnumerator CardSelectionSequence(SpecialNodeData nodeData)
		{
            Initialize();

			ViewManager.Instance.SwitchToView(View.Choices, false, true);
			yield return new WaitForSeconds(0.15f);

            SideDecksPlugin.Log.LogInfo($"Leshy is ready to give us side deck options");

            ChallengeActivationUI.Instance.ShowActivation(ChallengeID);

			yield return TextDisplayer.Instance.PlayDialogueEvent("SideDeckIntro", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);

            TableRuleBook.Instance.SetOnBoard(true);
			
            // We will deal out the boons in rows of four.
            // But we have to choose where the cards go based on how many there are.
			base.selectableCards = new List<SelectableCard>();
            
            List<string> allValidCards = SideDeckManager.GetAllValidSideDeckCards();
            allValidCards.Remove("Squirrel");

            SideDecksPlugin.Log.LogInfo($"All known side deck cards: {string.Join(",", allValidCards)}");

            this.selectedCardName = default(string);

            int i = 0;
			foreach (string cardName in allValidCards)
			{
				this.CreateSideDeckCard(i, allValidCards.Count, cardName);
				yield return new WaitForSeconds(0.1f);
                i += 1;
			}

            base.SetCollidersEnabled(true);
            base.EnableViewDeck(ViewController.ControlMode.CardChoice, this.basePosition); // Let the users see their deck when making this decision
            yield return new WaitUntil(() => !string.IsNullOrEmpty(this.selectedCardName)); // Wait until they've selected a card

            // And now the cleanup
            base.DisableViewDeck();
            this.CleanUpCards(); // This removes all of the cards we didn't use
            
            TableRuleBook.Instance.SetOnBoard(false);

            // Set the side deck
            SideDeckManager.SelectedSideDeck = this.selectedCardName;

			SaveManager.SaveToFile(false);

            if (GameFlowManager.Instance != null)
			{
				GameFlowManager.Instance.TransitionToGameState(GameState.Map, null);
			}

			yield break;
        }

        private Vector3 GetLocationForCard(int index, int totalCards)
        {
            int totalRows = (int)Math.Ceiling(((float)totalCards / 4f));
            int totalColumns = 4;

            if (totalCards < 4)
                totalColumns = totalCards;
            if (totalCards == 5 || totalCards == 6 || totalCards == 9)
                totalColumns = 3;

            Vector3 retval = BASE_ANCHOR;

            float rowOffset = totalRows == 1 ? 1f : totalRows == 2 ? 0.5f : 0f;
            float colOffset = totalColumns == 4 ? 0f : totalColumns == 3 ? 0.5f : totalColumns == 2 ? 1f : 1.5f;

            rowOffset += index / totalColumns;
            colOffset += index % totalColumns;

            return BASE_ANCHOR + Vector3.Scale(ROW_OFFSET, new Vector3(0f, 0f, rowOffset)) + Vector3.Scale(COL_OFFSET, new Vector3(colOffset, 0f, 0f));
        }

		// This creates a selectable upgrade card on the game mat
		private void CreateSideDeckCard(int index, int totalCards, string cardName, float tweenDelay = 0f)
		{
			GameObject newUpgradeCard = UnityEngine.Object.Instantiate<GameObject>(this.selectableCardPrefab, this.transform);
			SelectableCard component = newUpgradeCard.GetComponent<SelectableCard>();
			component.gameObject.SetActive(true);

            SideDecksPlugin.Log.LogInfo($"Starting to create card");

            // Need to create a decal card now
            CardInfo sideDeckCard = CardLoader.GetCardByName(cardName);
			component.Initialize(
                sideDeckCard,
                new Action<SelectableCard>(this.OnCardChosen),
                new Action<SelectableCard>(this.OnCardFlipped),
                true,
                new Action<SelectableCard>(base.OnCardInspected)
            );
            component.SetFaceDown(!ProgressionData.IntroducedCard(sideDeckCard));

            SideDecksPlugin.Log.LogInfo($"Calculating card location");  

			Vector3 vector = GetLocationForCard(index, totalCards);
			newUpgradeCard.transform.localPosition = vector + Vector3.forward * 6f;
			newUpgradeCard.transform.localScale = Vector3.Scale(newUpgradeCard.transform.localScale, new Vector3(0.8f, 0.8f, 1f));

			Tween.LocalPosition(newUpgradeCard.transform, vector, 0.2f, tweenDelay, Tween.EaseOut, Tween.LoopType.None, null, null, true);
			Tween.Rotate(newUpgradeCard.transform, new Vector3(0f, 0f, -2f + UnityEngine.Random.value * 4f), Space.Self, 0.25f, tweenDelay, Tween.EaseOut, Tween.LoopType.None, null, null, true);
			CustomCoroutine.WaitThenExecute(tweenDelay, new Action(component.Anim.PlayRiffleSound), false);

            SideDecksPlugin.Log.LogInfo($"The card is on the table");

            this.selectableCards.Add(component);

            SideDecksPlugin.Log.LogInfo($"Done");
		}

        private void OnCardChosen(SelectableCard card)
        {
            if (this.selectedCardName == null)
			{
				base.SetCollidersEnabled(false);
				this.selectedCardName = card.Info.name;
			}
        }

        private void OnCardFlipped(SelectableCard card)
        {
			if (card.Info != null)
			{
				base.StartCoroutine(this.TutorialTextSequence(card));
			}
        }

        private IEnumerator TutorialTextSequence(SelectableCard card)
		{
            if (!ProgressionData.IntroducedCard(card.Info))
            {
                if (!string.IsNullOrEmpty(card.Info.description))
                {
                    ViewManager.Instance.Controller.LockState = ViewLockState.Locked;
                    RuleBookController.Instance.SetShown(false, true);
                    yield return Singleton<TextDisplayer>.Instance.ShowUntilInput(card.Info.description, -2.5f, 0.5f, Emotion.Neutral, TextDisplayer.LetterAnimation.Jitter, DialogueEvent.Speaker.Single, null, true);
                    ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;
                }
                ProgressionData.SetCardIntroduced(card.Info);
            }
			yield break;
		}

        protected void CleanUpCards()
		{
			base.ResetLocalRotations();
			foreach (SelectableCard selectableCard in this.selectableCards)
			{
				selectableCard.SetInteractionEnabled(false);
				Tween.Position(selectableCard.transform, selectableCard.transform.position + Vector3.forward * 20f, 0.5f, 0f, Tween.EaseInOut, Tween.LoopType.None, null, null, true);
			    UnityEngine.Object.Destroy(selectableCard.gameObject, 0.5f);
			}
			this.selectableCards.Clear();
		}

        public IEnumerator ExecuteCustomSequence(CustomNodeData nodeData)
        {
            yield return CardSelectionSequence(nodeData);
        }

        private readonly Vector3 BASE_ANCHOR = new Vector3(-2.2f, 5.01f, -0.12f);

        private readonly Vector3 ROW_OFFSET = new Vector3(0f, 0f, -1.6f);

        private readonly Vector3 COL_OFFSET = new Vector3(1.6f, 0f, 0f);
    }
}