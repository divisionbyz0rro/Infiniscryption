using System;
using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using Infiniscryption.StarterDecks.Helpers;
using Infiniscryption.StarterDecks.Patchers;
using Pixelplacement;
using UnityEngine;
using HarmonyLib;
using Infiniscryption.Core.Helpers;
using Sirenix.OdinInspector;
using TMPro;

namespace Infiniscryption.StarterDecks.Sequences
{
    public class SpendExcessTeethSequencer : ManagedBehaviour
	{
		// We want this sequencer to behave almost exactly as the buy pelts sequencer
        // A few changes of course.
        // 1) The number of teeth to spend is based on the ExcessTeeth metacurrency
        // 2) What you're buying is not pelts: it's upgrades to your starter decks
        // 2a) This means that what you buy doesn't get added to your deck: it modifies the StarterDecks
        //     Obviously if you do this before you pick your starter deck, you'll get those upgrades on your
        //     current run
        // 3) When you leave, you don't go to the map - you go back to the skull (or at least stand up)

        // Yes, in fact, I did copy the code from BuyPeltsSequencer, then (heavily?) modify it.
        // I'm not good enough to do this on my own from scratch!
        
        private static Traverse _parentContainer;
        private static T GetCopiedField<T>(string fieldName) where T : class
        {
            if (_parentContainer == null)
                _parentContainer = Traverse.Create(Traverse.Create(SpecialNodeHandler.Instance).Field("buyPeltsSequencer").GetValue() as BuyPeltsSequencer);

            return _parentContainer.Field(fieldName).GetValue() as T;
        }

		private List<List<SelectableCard>> dealtDecks;

		public IEnumerator SpendExcessTeeth()
		{
			ViewManager.Instance.SwitchToView(View.Default, false, true);
			yield return new WaitForSeconds(0.15f);

            LeshyAnimationController.Instance.PutOnMask(LeshyAnimationController.Mask.Trader, true);
            yield return new WaitForSeconds(1.5f);

            InfiniscryptionStarterDecksPlugin.Log.LogInfo($"Leshy is ready to sell upgrades");

            // Tell the player that they can spend their ancestor's teeth here
			yield return TextDisplayer.Instance.PlayDialogueEvent("AlertSpendTeeth", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);

            if (RunState.DeckList.Count > 0)
                yield return TextDisplayer.Instance.PlayDialogueEvent("AlertOnlyForNewRun", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);

            yield return new WaitForSeconds(0.5f);

            // Look at the view list
			ViewManager.Instance.SwitchToView(View.TradingTopDown, false, false);
			yield return new WaitForSeconds(0.3f);

			InfiniscryptionStarterDecksPlugin.Log.LogInfo($"Showing existing decks...");
			// There are three decks, four cards each.
			// We'll deal them out as three groups of four.
			// Each group of four will be a 2x2.
			// We'll deal each deck at a time
			dealtDecks = new List<List<SelectableCard>>();
			for (int i = 0; i < DeckConstructionPatches.StarterDecks.Count; i++)
			{
				DealDeck(i);
			}

            InfiniscryptionStarterDecksPlugin.Log.LogInfo($"Setting up cards");
            
            // Set up the selectable cards
			this.upgradesForSale = new List<SelectableCard>{null,null,null};
            this.upgradePrices = new List<int>{0, 0, 0};
			for (int i = 0; i < DeckConstructionPatches.StarterDecks.Count; i++)
			{
				this.CreateUpgradeCard(i, 0f);
				yield return new WaitForSeconds(0.1f);
			}

            // For now, let's not show the currency/teeth.
            // We'll add that back in later
			//yield return CurrencyBowl.Instance.SpillOnTable();

			// Generate the confirmstone button
            this._confirmStoneButton = GameObject.Instantiate<GameObject>(ResourceBank.Get<GameObject>("Prefabs/SpecialNodeSequences/ConfirmStoneButton"));
            this._confirmStoneButton.transform.localPosition += new Vector3(3.3f, 0f, -1f);
			this._confirmStoneButton.transform.localRotation = Quaternion.AngleAxis(90, Vector3.up);

            foreach (Component comp in _confirmStoneButton.GetComponentsInChildren<Component>())
            {
                if (comp is MeshRenderer && comp.name == "Quad")
                    (comp as MeshRenderer).material.mainTexture = Resources.Load<Texture>("art/cards/statboost_button");
            }

            ConfirmStoneButton btn = _confirmStoneButton.GetComponentInChildren<ConfirmStoneButton>();
            btn.HighlightCursorType = CursorType.Slap;
            btn.SetColors(
                GameColors.Instance.limeGreen,
                GameColors.Instance.limeGreen, // interactable color
                GameColors.Instance.darkLimeGreen // hover color
            );
            btn.SetButtonInteractable();

			ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;
			ViewManager.Instance.Controller.SwitchToControlMode(ViewController.ControlMode.Trading);
			ViewManager.Instance.SwitchToView(View.TradingTopDown, false, true);
			
            // Wait until you back away from the table
			yield return btn.WaitUntilConfirmation();

			GameObject.Destroy(this._confirmStoneButton);

            bool playOuttroText = this.purchasedUpgrades.Count > 0;

			this.purchasedUpgrades.Clear();

			// Destroy the purchaseable cards
			foreach (SelectableCard selectableCard in this.upgradesForSale)
			{
				int num2 = this.upgradesForSale.IndexOf(selectableCard);
				Tween.LocalPosition(selectableCard.transform, selectableCard.transform.localPosition + Vector3.forward * 3f, 0.2f, 0.05f * (float)num2, Tween.EaseIn, Tween.LoopType.None, null, null, true);
				selectableCard.SetEnabled(false);
				UnityEngine.Object.Destroy(selectableCard.gameObject, 0.35f);
			}

			// Move all the dealt cards to a pile where they'll be destroyed later. 
			foreach (var deck in this.dealtDecks)
			{
				foreach (SelectableCard selectableCard in deck)
				{
					selectableCard.Anim.PlayQuickRiffleSound();
					this.deckPile.MoveCardToPile(selectableCard, false, 0f, 0.7f);
					this.deckPile.AddToPile(selectableCard.transform);
				}
			}

			this.upgradesForSale.Clear();

			//yield return this.StartCoroutine(CurrencyBowl.Instance.CleanUpFromTableAndExit());

			ViewManager.Instance.SwitchToView(View.Default, false, false);
			yield return new WaitForSeconds(0.1f);
            if (playOuttroText)
			    yield return TextDisplayer.Instance.PlayDialogueEvent("UpgradedStarterDecks", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
			yield return LeshyAnimationController.Instance.TakeOffMask();

			yield return new WaitForSeconds(1.5f);
			yield return this.StartCoroutine(this.deckPile.DestroyCards(0.5f));

			// Reset the first map node
			// This makes sure that this affects your deck selection if your run hasn't
			// really started yet
			ResetFirstMapNode();
			
			ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;
			if (GameFlowManager.Instance != null)
			{
                // And we go back to 3D! Hopefully we're still pointing at the skull!
				GameFlowManager.Instance.TransitionToGameState(GameState.FirstPerson3D, null);
				OpponentAnimationController.Instance.SetExplorationMode(true);
			}

			SaveManager.SaveToFile(false);

			yield break;
        }

		private void DealDeck(int index, float tweenDelay = 0f)
		{
			InfiniscryptionStarterDecksPlugin.Log.LogInfo($"Dealing deck {index}");

			// Create a new cardpile
			List<SelectableCard> deckPiles = new List<SelectableCard>();

			// Get the decklist
			List<CardInfo> decklist = CardManagementHelper.EvolveDeck(index);

			for (int j = 0; j < 4; j++)
			{
				InfiniscryptionStarterDecksPlugin.Log.LogInfo($"Dealing deck {index} card {j} {decklist[j].name}");

				// Okay, let's initialize a new cardpile through duplication
				GameObject newCard = UnityEngine.Object.Instantiate<GameObject>(this.selectableCardPrefab, this.transform);
				SelectableCard component = newCard.GetComponent<SelectableCard>();
				component.gameObject.SetActive(true);
				component.SetInfo(decklist[j]);
				component.SetEnabled(false);

				// Figure out where it goes
				Vector3 vector = this.DECK_ANCHOR + this.DECK_SPACING * (float)index + this.DECK_INTERNAL_SPACING[j];
				newCard.transform.localPosition = vector + Vector3.forward * 3f;

				// Please work
				newCard.transform.localScale = Vector3.Scale(newCard.transform.localScale, new Vector3(0.8f, 0.8f, 1f));

				// Activate
				Tween.LocalPosition(newCard.transform, vector, 0.2f, tweenDelay, Tween.EaseOut, Tween.LoopType.None, null, null, true);
				Tween.Rotate(newCard.transform, new Vector3(0f, 0f, -2f + UnityEngine.Random.value * 4f), Space.Self, 0.25f, tweenDelay, Tween.EaseOut, Tween.LoopType.None, null, null, true);
				CustomCoroutine.WaitThenExecute(tweenDelay, new Action(component.Anim.PlayRiffleSound), false);

				deckPiles.Add(component);
			}
			dealtDecks.Add(deckPiles);
		}

		// This creates a selectable upgrade card on the game mat
		private void CreateUpgradeCard(int index, float tweenDelay = 0f)
		{
            InfiniscryptionStarterDecksPlugin.Log.LogInfo($"Starting 'CreateUpgradeCard' and creating from prefab");

			GameObject newUpgradeCard = UnityEngine.Object.Instantiate<GameObject>(this.selectableCardPrefab, this.transform);
			SelectableCard component = newUpgradeCard.GetComponent<SelectableCard>();
			component.gameObject.SetActive(true);

            InfiniscryptionStarterDecksPlugin.Log.LogInfo($"Getting upgrade card {index} for deck {DeckConstructionPatches.StarterDecks[index]} with evolution {DeckConstructionPatches.StarterDeckEvolutions[index]} at stage {DeckConstructionPatches.DeckEvolutionProgress[index]}");

            // The card that you can buy is the next upgrade in the sequence
			CardInfo cardByName = CardManagementHelper.GetNextEvolution(index);

            InfiniscryptionStarterDecksPlugin.Log.LogInfo($"The upgrade card name is {cardByName.name}");

			component.SetInfo(cardByName);
            
            // The price is 10 + 5 * deck evolution progess
            // TODO: Put this somewhere else!
            int price = (DeckConstructionPatches.DeckEvolutionProgress[index] + 1) * InfiniscryptionStarterDecksPlugin.CostPerLevel;

            InfiniscryptionStarterDecksPlugin.Log.LogInfo($"The price of the card name is {price}");
			if (price > 0)
			{
				this.AddPricetagToCard(component, price, tweenDelay);
			}

            InfiniscryptionStarterDecksPlugin.Log.LogInfo($"The price tag has been added");

			Vector3 vector = this.UPGRADES_ANCHOR + this.UPGRADE_SPACING * (float)index;
			newUpgradeCard.transform.localPosition = vector + Vector3.forward * 3f;
			newUpgradeCard.transform.localScale = Vector3.Scale(newUpgradeCard.transform.localScale, new Vector3(0.8f, 0.8f, 1f));

			Tween.LocalPosition(newUpgradeCard.transform, vector, 0.2f, tweenDelay, Tween.EaseOut, Tween.LoopType.None, null, null, true);
			Tween.Rotate(newUpgradeCard.transform, new Vector3(0f, 0f, -2f + UnityEngine.Random.value * 4f), Space.Self, 0.25f, tweenDelay, Tween.EaseOut, Tween.LoopType.None, null, null, true);
			CustomCoroutine.WaitThenExecute(tweenDelay, new Action(component.Anim.PlayRiffleSound), false);

            InfiniscryptionStarterDecksPlugin.Log.LogInfo($"The card is on the table");

			// We need to know which of the other cards to replace with this guy
			// To do that, we look at the next evolution instruction that hasn't been applied yet
			// The first character is an integer of the card that gets replaced next
			// Add 4 * index because this is the index'th deck to be dealt on the table.
			int evo = DeckConstructionPatches.DeckEvolutionProgress[index];
			string[] evoCommands = DeckConstructionPatches.StarterDeckEvolutions[index].Split(',');
			string cmd = CardManagementHelper.GetEvolutionCommand(evoCommands, evo);
			int nextEvoIdx = int.Parse(cmd[0].ToString());

			component.CursorSelectStarted = (Action<MainInputInteractable>)Delegate.Combine(component.CursorSelectStarted, new Action<MainInputInteractable>(delegate(MainInputInteractable c)
			{
				this.TryBuyUpgrade(c as SelectableCard, index, nextEvoIdx);
			}));

			component.CursorEntered = (Action<MainInputInteractable>)Delegate.Combine(component.CursorEntered, new Action<MainInputInteractable>(delegate(MainInputInteractable c)
			{
				this.OnHoverUpgrade(index, nextEvoIdx, price);
			}));

			component.CursorExited = (Action<MainInputInteractable>)Delegate.Combine(component.CursorExited, new Action<MainInputInteractable>(delegate(MainInputInteractable c)
			{
				this.OnLeaveUpgrade(index, nextEvoIdx);
			}));

			this.upgradesForSale[index] = component;
            this.upgradePrices[index] = price;

            InfiniscryptionStarterDecksPlugin.Log.LogInfo($"Done");
		}

		// Token: 0x06000C01 RID: 3073 RVA: 0x0002BFA8 File Offset: 0x0002A1A8
		private void AddPricetagToCard(SelectableCard card, int price, float tweenDelay)
		{
			GameObject priceTag = UnityEngine.Object.Instantiate<GameObject>(this.pricetagPrefab);
			priceTag.transform.SetParent(card.transform);
			priceTag.transform.localPosition = new Vector3(-0.6f, 1.4f, -0.03f);
			priceTag.transform.localEulerAngles = new Vector3(-90f, -90f, 90f);
			priceTag.transform.localScale = Vector3.Scale(priceTag.transform.localScale, new Vector3(0.8f, 0.8f, 1f));
			priceTag.name = "pricetag";
			priceTag.GetComponentInChildren<Renderer>().material.mainTexture = this.pricetagTextures[0];

			// Now we need to add the price to the tag as a text mesh
			// Okay, I can't actually add it apparently? It gets really, really angry.
			// So instead it's a new gameobject, connected as a transform
			GameObject priceLabel = new GameObject();
			priceLabel.name = "pricelabel";
			priceLabel.transform.SetParent(priceTag.transform);
			TextMeshPro textMesh = priceLabel.AddComponent<TextMeshPro>();
			textMesh.autoSizeTextContainer = true;
			textMesh.text = price.ToString();
			textMesh.fontSize = 3;
			textMesh.transform.rotation = Quaternion.LookRotation(-Vector3.up, Vector3.up);
			textMesh.transform.localPosition = new Vector3(0.1f, 0.05f, -0.7f);
			textMesh.alignment = TextAlignmentOptions.Center;
			textMesh.font = this.pricetagFont;
			textMesh.color = Color.black;// new Color(0.4196f, 0.2275f, 0.1725f);

			Tween.LocalRotation(priceTag.transform, new Vector3(-80f + UnityEngine.Random.value * -20f, -90f, 90f), 0.25f, tweenDelay, Tween.EaseOut, Tween.LoopType.None, null, null, true);
		}

		private void GainUpgrade(SelectableCard upgrade, int deck, int cardSlot, float tweenDelay = 0f)
		{
            // This handles what happens when you have enough teeth to buy an upgrade

			int idx = this.upgradesForSale.IndexOf(upgrade);
			this.purchasedUpgrades.Add(upgrade);

            // We need to increase the current evolution!
            // Grabbing the upgrade card itself does nothing
            // It's just a representation of increasing the evolution progress counter
            DeckConstructionPatches.UpdateDeckEvolutionProgress(idx, DeckConstructionPatches.DeckEvolutionProgress[idx] + 1);

			this.CreateUpgradeCard(idx, 0.25f);

			// Get rid of the price tag
			Transform transform = upgrade.transform.Find("pricetag");
			if (transform != null)
			{
				transform.parent = null;
				Tween.Position(transform, transform.position + Vector3.forward + Vector3.up * 2f, 0.2f, 0f, Tween.EaseIn, Tween.LoopType.None, null, null, true);
				Tween.Rotate(transform, new Vector3(45f, 0f, 0f), Space.World, 0.2f, 0f, Tween.EaseIn, Tween.LoopType.None, null, null, true);
				
				Transform labelTransform = transform.transform.Find("pricelabel");
				if (labelTransform != null)
					UnityEngine.Object.Destroy(labelTransform.gameObject);

                UnityEngine.Object.Destroy(transform.gameObject, 0.4f);
			}
			
			upgrade.Anim.PlayRiffleSound();

			// We're going to find the card this replaces, get rid of that card
			SelectableCard cardToReplace = dealtDecks[deck][cardSlot];
			cardToReplace.SetFaceDown(true);
			Vector3 leaveTablePosition = cardToReplace.gameObject.transform.localPosition + new Vector3(0f, 4f, 0f);
			Tween.LocalPosition(cardToReplace.gameObject.transform, leaveTablePosition, 0.2f, tweenDelay, Tween.EaseOut, Tween.LoopType.None, null, null, true);

			CustomCoroutine.WaitThenExecute(1f, new Action(() => UnityEngine.GameObject.Destroy(cardToReplace.gameObject)), false);

			// Move the new card to the old spot.
			Vector3 vector = this.DECK_ANCHOR + this.DECK_SPACING * (float)deck + this.DECK_INTERNAL_SPACING[cardSlot];

			Tween.LocalPosition(upgrade.gameObject.transform, vector, 0.2f, tweenDelay, Tween.EaseOut, Tween.LoopType.None, null, null, true);
			Tween.Rotate(upgrade.gameObject.transform, new Vector3(0f, 0f, -2f + UnityEngine.Random.value * 4f), Space.Self, 0.25f, tweenDelay, Tween.EaseOut, Tween.LoopType.None, null, null, true);
			CustomCoroutine.WaitThenExecute(tweenDelay, new Action(upgrade.Anim.PlayRiffleSound), false);

			upgrade.SetEnabled(false);
			dealtDecks[deck][cardSlot] = upgrade;

			//upgrade.SetEnabled(false);
			//this.purchasedPile.MoveCardToPile(upgrade, false, 0f, 0.7f);
			//this.purchasedPile.AddToPile(upgrade.transform);
		}

		// Token: 0x06000C03 RID: 3075 RVA: 0x0002C1A4 File Offset: 0x0002A3A4
		private void TryBuyUpgrade(SelectableCard upgrade, int deck, int cardSlot)
		{
			int idx = this.upgradesForSale.IndexOf(upgrade);
			int price = this.upgradePrices[idx];

            // We care about the excess teeth from the metacurrency module
			if (MetaCurrencyPatches.ExcessTeeth >= price)
			{
                /*
				List<Rigidbody> list = CurrencyBowl.Instance.TakeWeights(price);
				foreach (Rigidbody rigidbody in list)
				{
					float num3 = (float)list.IndexOf(rigidbody) * 0.05f;
					Tween.Position(rigidbody.transform, rigidbody.transform.position + Vector3.up * 0.5f, 0.075f, num3, Tween.EaseIn, Tween.LoopType.None, null, null, true);
					Tween.Position(rigidbody.transform, new Vector3(0f, 5.5f, 4f), 0.3f, 0.125f + num3, Tween.EaseOut, Tween.LoopType.None, null, null, true);
					UnityEngine.Object.Destroy(rigidbody.gameObject, 0.5f);
				}
                */
				this.GainUpgrade(upgrade, deck, cardSlot);
				MetaCurrencyPatches.ExcessTeeth -= price;
				return;
			}
			else if (!TextDisplayer.Instance.Displaying)
			{
				this.StartCoroutine(TextDisplayer.Instance.ShowThenClear("You'll need more teeth for that one.", 3f, 0f, Emotion.Neutral, TextDisplayer.LetterAnimation.Jitter, DialogueEvent.Speaker.Single, null));
			}
		}

		private void HighlightCard(int deck, int card, bool up=true, float tweenDelay = 0f)
		{
			SelectableCard replaceCard = this.dealtDecks[deck][card];
			GameObject moveObj = replaceCard.gameObject;

			Vector3 expectedPos = this.DECK_ANCHOR + this.DECK_SPACING * (float)deck + this.DECK_INTERNAL_SPACING[card];

			Vector3 vector = up ? expectedPos + new Vector3(0f, 0.2f, 0f) : expectedPos;
			Tween.LocalPosition(moveObj.transform, vector, 0.2f, tweenDelay, Tween.EaseOut, Tween.LoopType.None, null, null, true);
			Tween.Rotate(moveObj.transform, new Vector3(0f, 0f, -2f + UnityEngine.Random.value * 4f), Space.Self, 0.25f, tweenDelay, Tween.EaseOut, Tween.LoopType.None, null, null, true);
			CustomCoroutine.WaitThenExecute(tweenDelay, new Action(replaceCard.Anim.PlayRiffleSound), false);
		}

		// Token: 0x06000C07 RID: 3079 RVA: 0x0002C4B0 File Offset: 0x0002A6B0
		private void OnHoverUpgrade(int deck, int card, int amount)
		{
			HighlightCard(deck, card, true);
		}

		// Token: 0x06000C08 RID: 3080 RVA: 0x0002C563 File Offset: 0x0002A763
		private void OnLeaveUpgrade(int deck, int card)
		{
			HighlightCard(deck, card, false);
		}

		public static void ResetFirstMapNode()
        {
            // This helper resets the override choices on the first map node
            // If your deck is empty
            if (RunState.Run.playerDeck.Cards.Count == 0)
            {
                // The first node of the map will be a CardChoicesNode
                // Well, actually the first node will be an empty node.
                // Then a card choices node
                NodeData firstNonEmptyNode  = RunState.Run.map.nodeData.Find(node => node.gridY == 1);

                if (firstNonEmptyNode is CardChoicesNodeData)
                {
                    CardChoicesNodeData target = firstNonEmptyNode as CardChoicesNodeData;

                    for (int i = 0; i < DeckConstructionPatches.StarterDecks.Count; i++)
                    {
                        List<CardInfo> deck = CardManagementHelper.EvolveDeck(i);
                        InfiniscryptionStarterDecksPlugin.Log.LogInfo($"Adding choice {i}: {deck[0].name} with {deck[0].Mods.Count} mods");
                        target.overrideChoices[i].CardInfo = deck[0];
                    }
                }
            }
        }

        private CardPile _deckPile;
		private CardPile deckPile
        {
            get 
            {
                if (_deckPile == null)
                    _deckPile = GetCopiedField<CardPile>("deckPile");

                return _deckPile;
            }
        }        

		private Transform _weightOrganizeAnchor;
		private Transform weightOrganizeAnchor
        {
            get 
            {
                if (_weightOrganizeAnchor == null)
                    _weightOrganizeAnchor = GetCopiedField<Transform>("weightOrganizeAnchor");

                return _weightOrganizeAnchor;
            }
        }

		private GameObject _selectableCardPrefab;
		private GameObject selectableCardPrefab
        {
            get 
            {
                if (_selectableCardPrefab == null)
                    _selectableCardPrefab = GetCopiedField<GameObject>("selectableCardPrefab");

                return _selectableCardPrefab;
            }
        }

		private GameObject _pricetagPrefab;
		private GameObject pricetagPrefab
        {
            get 
            {
                if (_pricetagPrefab == null)
                    _pricetagPrefab = GetCopiedField<GameObject>("pricetagPrefab");

                return _pricetagPrefab;
            }
        }

		private Texture[] _pricetagTextures;
		private Texture[] pricetagTextures
        {
            get 
            {
                if (_pricetagTextures == null)
                    _pricetagTextures = GetCopiedField<Texture[]>("pricetagTextures");

                return _pricetagTextures;
            }
        }

		private TMP_FontAsset pricetagFont = Resources.Load<TMP_FontAsset>("fonts/3d scene fonts/garbageschrift");

		private readonly Vector3 SCALE_FACTOR = new Vector3(0.8f, 0.8f, 1f);

		private readonly Vector3 UPGRADES_ANCHOR = new Vector3(-2.5f, 5.01f, -0.12f);

		private readonly Vector3 UPGRADE_SPACING = new Vector3(0f, 0f, -1.7f);

		private readonly Vector3 DECK_ANCHOR = new Vector3(-0.6f, 5.01f, -0.12f);

		private readonly Vector3 DECK_SPACING = new Vector3(0f, 0f, -1.7f);

		private readonly Vector3[] DECK_INTERNAL_SPACING = new Vector3[]
		{
			new Vector3(0f, 0f, 0f),
			new Vector3(1.1f, 0f, 0f),
			new Vector3(2.2f, 0f, 0f),
			new Vector3(3.3f, 0f, 0f)
		};

		private List<SelectableCard> upgradesForSale;

        private List<int> upgradePrices;

		private List<SelectableCard> purchasedUpgrades = new List<SelectableCard>();

		private GameObject _confirmStoneButton;
    }
}