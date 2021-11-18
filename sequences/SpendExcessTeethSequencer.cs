using System;
using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using Infiniscryption.Helpers;
using Infiniscryption.Patchers;
using Pixelplacement;
using UnityEngine;
using HarmonyLib;
using Sirenix.OdinInspector;
using TMPro;

namespace Infiniscryption.Sequences
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

        // This is the singleton instance of this thing we should always be using
        
        /*
        private static SpendExcessTeethSequencer _instance;
        public static SpendExcessTeethSequencer Instance
        {
            get
            {
                // This enusres that the singleton instance doesn't get created
                // until the first time we need it.
                if (_instance == null)
                    Instantiate();

                return _instance;
            }
        }
        */

        private static Traverse _parentContainer;
        private static T GetCopiedField<T>(string fieldName) where T : class
        {
            if (_parentContainer == null)
                _parentContainer = Traverse.Create(Traverse.Create(SpecialNodeHandler.Instance).Field("buyPeltsSequencer").GetValue() as BuyPeltsSequencer);

            return _parentContainer.Field(fieldName).GetValue() as T;
        }

		public IEnumerator SpendExcessTeeth()
		{
			Singleton<ViewManager>.Instance.SwitchToView(View.Default, false, true);
			yield return new WaitForSeconds(0.15f);

            LeshyAnimationController.Instance.PutOnMask(LeshyAnimationController.Mask.Trader, true);
            yield return new WaitForSeconds(1.5f);

            InfiniscryptionMetaCurrencyPlugin.Log.LogInfo($"Leshy is ready to sell upgrades");

            // Tell the player that they can spend their ancestor's teeth here
			yield return Singleton<TextDisplayer>.Instance.PlayDialogueEvent("AlertSpendTeeth", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);

            if (RunState.DeckList.Count > 0)
                yield return Singleton<TextDisplayer>.Instance.PlayDialogueEvent("AlertOnlyForNewRun", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);

            yield return new WaitForSeconds(0.5f);

            // Look at the view list
			Singleton<ViewManager>.Instance.SwitchToView(View.TradingTopDown, false, false);
			yield return new WaitForSeconds(0.3f);

            InfiniscryptionMetaCurrencyPlugin.Log.LogInfo($"Setting up cards");
            
            // Set up the selectable cards
			this.upgradesForSale = new List<SelectableCard>{null,null,null};
            this.upgradePrices = new List<int>{0, 0, 0};
			for (int i = 0; i < DeckConstructionPatches.StarterDecks.Count; i++)
			{
				this.CreateUpgradeCard(i, 0f);
				yield return new WaitForSeconds(0.1f);
			}

			this.weightsBlocker.gameObject.SetActive(true);

            // For now, let's not show the currency/teeth.
            // We'll add that back in later
			//yield return Singleton<CurrencyBowl>.Instance.SpillOnTable();
			Singleton<ViewManager>.Instance.Controller.LockState = ViewLockState.Unlocked;
            Singleton<ViewManager>.Instance.Controller.SwitchToControlMode(ViewController.ControlMode.Trading, false);

			bool tradingCompleted = false;
			this.purchasedPile.SetEnabled(true);
			this.EnableWeightInteraction();
			
            // This sets up the pile of purchased cards and waits until the user clicks it (indicating that they're done)
			CardPile cardPile2 = this.purchasedPile;
			cardPile2.CursorSelectEnded = (Action<MainInputInteractable>)Delegate.Combine(cardPile2.CursorSelectEnded, new Action<MainInputInteractable>(delegate(MainInputInteractable i)
			{
				tradingCompleted = true;
			}));
			yield return new WaitUntil(() => tradingCompleted);


			Singleton<ViewManager>.Instance.SwitchToView(View.TradingTopDown, false, true);
			this.DisableWeightInteraction();
			this.purchasedPile.SetEnabled(false);
			this.purchasedPile.ClearDelegates();

            // Shuffle the selected cards for fun
			for (int i = this.purchasedUpgrades.Count - 1; i >= 0; i--)
			{
				this.purchasedUpgrades[i].Anim.PlayQuickRiffleSound();
				this.purchasedUpgrades[i].SetFaceDown(true, false);
				yield return new WaitForSeconds(0.05f);
				this.deckPile.MoveCardToPile(this.purchasedUpgrades[i], false, 0f, 0.7f);
				this.deckPile.AddToPile(this.purchasedUpgrades[i].transform);
				yield return new WaitForSeconds(0.05f);
			}

            bool playOuttroText = this.purchasedUpgrades.Count > 0;

			this.purchasedUpgrades.Clear();
			foreach (SelectableCard selectableCard in this.upgradesForSale)
			{
				int num2 = this.upgradesForSale.IndexOf(selectableCard);
				Tween.LocalPosition(selectableCard.transform, selectableCard.transform.localPosition + Vector3.forward * 3f, 0.2f, 0.05f * (float)num2, Tween.EaseIn, Tween.LoopType.None, null, null, true);
				selectableCard.SetEnabled(false);
				UnityEngine.Object.Destroy(selectableCard.gameObject, 0.35f);
			}

			this.upgradesForSale.Clear();
			this.weightsBlocker.gameObject.SetActive(false);

			//yield return this.StartCoroutine(Singleton<CurrencyBowl>.Instance.CleanUpFromTableAndExit());

			Singleton<ViewManager>.Instance.SwitchToView(View.Default, false, false);
			yield return new WaitForSeconds(0.1f);
            if (playOuttroText)
			    yield return Singleton<TextDisplayer>.Instance.PlayDialogueEvent("UpgradedStarterDecks", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
			yield return LeshyAnimationController.Instance.TakeOffMask();

			yield return new WaitForSeconds(0.25f);
			yield return this.StartCoroutine(this.deckPile.DestroyCards(0.5f));

			// Reset the first map node
			// This makes sure that this affects your deck selection if your run hasn't
			// really started yet
			ResetFirstMapNode();
			
			Singleton<ViewManager>.Instance.Controller.LockState = ViewLockState.Unlocked;
			if (Singleton<GameFlowManager>.Instance != null)
			{
                // And we go back to 3D! Hopefully we're still pointing at the skull!
				Singleton<GameFlowManager>.Instance.TransitionToGameState(GameState.FirstPerson3D, null);
			}
			yield break;
        }

		// This creates a selectable upgrade card on the game mat
		private void CreateUpgradeCard(int index, float tweenDelay = 0f)
		{
            InfiniscryptionMetaCurrencyPlugin.Log.LogInfo($"Starting 'CreateUpgradeCard' and creating from prefab");

			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.selectableCardPrefab, this.transform);
			SelectableCard component = gameObject.GetComponent<SelectableCard>();
			component.gameObject.SetActive(true);

            InfiniscryptionMetaCurrencyPlugin.Log.LogInfo($"Getting upgrade card {index} for deck {DeckConstructionPatches.StarterDecks[index]} with evolution {DeckConstructionPatches.StarterDeckEvolutions[index]} at stage {DeckConstructionPatches.DeckEvolutionProgress[index]}");

            // The card that you can buy is the next upgrade in the sequence
			CardInfo cardByName = CardManagementHelper.GetNextEvolution(index);

            InfiniscryptionMetaCurrencyPlugin.Log.LogInfo($"The upgrade card name is {cardByName.name}");

			component.SetInfo(cardByName);
            
            // The price is 10 + 5 * deck evolution progess
            // TODO: Put this somewhere else!
            int price = 10 + DeckConstructionPatches.DeckEvolutionProgress[index] * 5;

            InfiniscryptionMetaCurrencyPlugin.Log.LogInfo($"The price of the card name is {price}");
			if (price > 0)
			{
				this.AddPricetagToCard(component, price, tweenDelay);
			}

            InfiniscryptionMetaCurrencyPlugin.Log.LogInfo($"The price tag has been added");

			Vector3 vector = this.UPGRADES_ANCHOR + this.UPGRADE_SPACING * (float)index;
			gameObject.transform.localPosition = vector + Vector3.forward * 3f;
			Tween.LocalPosition(gameObject.transform, vector, 0.2f, tweenDelay, Tween.EaseOut, Tween.LoopType.None, null, null, true);
			Tween.Rotate(gameObject.transform, new Vector3(0f, 0f, -2f + UnityEngine.Random.value * 4f), Space.Self, 0.25f, tweenDelay, Tween.EaseOut, Tween.LoopType.None, null, null, true);
			CustomCoroutine.WaitThenExecute(tweenDelay, new Action(component.Anim.PlayRiffleSound), false);

            InfiniscryptionMetaCurrencyPlugin.Log.LogInfo($"The card is on the table");

			component.CursorSelectStarted = (Action<MainInputInteractable>)Delegate.Combine(component.CursorSelectStarted, new Action<MainInputInteractable>(delegate(MainInputInteractable c)
			{
				this.TryBuyUpgrade(c as SelectableCard);
			}));

            /* currencybowl
			component.CursorEntered = (Action<MainInputInteractable>)Delegate.Combine(component.CursorEntered, new Action<MainInputInteractable>(delegate(MainInputInteractable c)
			{
				this.ShowWeightsHighlighted(price);
			}));

			component.CursorExited = (Action<MainInputInteractable>)Delegate.Combine(component.CursorExited, new Action<MainInputInteractable>(delegate(MainInputInteractable c)
			{
				this.StopWeightsHighlighted();
			}));
            */

			this.upgradesForSale[index] = component;
            this.upgradePrices[index] = price;

            InfiniscryptionMetaCurrencyPlugin.Log.LogInfo($"Done");
		}

		// Token: 0x06000C01 RID: 3073 RVA: 0x0002BFA8 File Offset: 0x0002A1A8
		private void AddPricetagToCard(SelectableCard card, int price, float tweenDelay)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.pricetagPrefab);
			gameObject.transform.SetParent(card.transform);
			gameObject.transform.localPosition = new Vector3(-0.6f, 1.4f, -0.03f);
			gameObject.transform.localEulerAngles = new Vector3(-90f, -90f, 90f);
			gameObject.name = "pricetag";
			gameObject.GetComponentInChildren<Renderer>().material.mainTexture = this.pricetagTextures[0];

			// Now we need to add the price to the tag as a text mesh
			// Okay, I can't actually add it apparently? It gets really, really angry.
			GameObject priceLabel = new GameObject();
			priceLabel.name = "pricelabel";
			priceLabel.transform.SetParent(gameObject.transform);
			TextMeshPro textMesh = priceLabel.AddComponent<TextMeshPro>();
			textMesh.autoSizeTextContainer = true;
			textMesh.text = price.ToString();
			textMesh.fontSize = 5;
			textMesh.transform.rotation = Quaternion.LookRotation(-Vector3.up, Vector3.up);
			textMesh.transform.localPosition = new Vector3(0f, 0.05f, -0.7f);
			textMesh.alignment = TextAlignmentOptions.Center;
			textMesh.font = this.pricetagFont;
			textMesh.color = new Color(0.4196f, 0.2275f, 0.1725f);

			Tween.LocalRotation(gameObject.transform, new Vector3(-80f + UnityEngine.Random.value * -20f, -90f, 90f), 0.25f, tweenDelay, Tween.EaseOut, Tween.LoopType.None, null, null, true);
		}

		private void GainUpgrade(SelectableCard upgrade)
		{
            // This handles what happens when you have enough teeth to buy an upgrade

			int idx = this.upgradesForSale.IndexOf(upgrade);
			this.purchasedUpgrades.Add(upgrade);

            // We need to increase the current evolution!
            // Grabbing the upgrade card itself does nothing
            // It's just a representation of increasing the evolution progress counter
            DeckConstructionPatches.UpdateDeckEvolutionProgress(idx, DeckConstructionPatches.DeckEvolutionProgress[idx] + 1);

			this.CreateUpgradeCard(idx, 0.25f);
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
			upgrade.SetEnabled(false);
			upgrade.Anim.PlayRiffleSound();
			this.purchasedPile.MoveCardToPile(upgrade, false, 0f, 0.7f);
			this.purchasedPile.AddToPile(upgrade.transform);
		}

		// Token: 0x06000C03 RID: 3075 RVA: 0x0002C1A4 File Offset: 0x0002A3A4
		private void TryBuyUpgrade(SelectableCard upgrade)
		{
			int idx = this.upgradesForSale.IndexOf(upgrade);
			int price = this.upgradePrices[idx];

            // We care about the excess teeth from the metacurrency module
			if (MetaCurrencyPatches.ExcessTeeth >= price)
			{
                /*
				List<Rigidbody> list = Singleton<CurrencyBowl>.Instance.TakeWeights(price);
				foreach (Rigidbody rigidbody in list)
				{
					float num3 = (float)list.IndexOf(rigidbody) * 0.05f;
					Tween.Position(rigidbody.transform, rigidbody.transform.position + Vector3.up * 0.5f, 0.075f, num3, Tween.EaseIn, Tween.LoopType.None, null, null, true);
					Tween.Position(rigidbody.transform, new Vector3(0f, 5.5f, 4f), 0.3f, 0.125f + num3, Tween.EaseOut, Tween.LoopType.None, null, null, true);
					UnityEngine.Object.Destroy(rigidbody.gameObject, 0.5f);
				}
                */
				this.GainUpgrade(upgrade);
				MetaCurrencyPatches.ExcessTeeth -= price;
				return;
			}
			else if (!Singleton<TextDisplayer>.Instance.Displaying)
			{
				this.StartCoroutine(Singleton<TextDisplayer>.Instance.ShowThenClear("You'll need more teeth for that one.", 3f, 0f, Emotion.Neutral, TextDisplayer.LetterAnimation.Jitter, DialogueEvent.Speaker.Single, null));
			}
		}

		// Token: 0x06000C04 RID: 3076 RVA: 0x0002C304 File Offset: 0x0002A504
		private void EnableWeightInteraction()
		{
            /*
			foreach (Rigidbody rigidbody in Singleton<CurrencyBowl>.Instance.ActiveWeights)
			{
				GenericMainInputInteractable genericMainInputInteractable = rigidbody.gameObject.AddComponent<GenericMainInputInteractable>();
				genericMainInputInteractable.SetCursorType(CursorType.Point);
				genericMainInputInteractable.CursorSelectStarted = (Action<MainInputInteractable>)Delegate.Combine(genericMainInputInteractable.CursorSelectStarted, new Action<MainInputInteractable>(delegate(MainInputInteractable i)
				{
					this.OrganizeWeights();
				}));
			}
            */
		}

		// Token: 0x06000C05 RID: 3077 RVA: 0x0002C388 File Offset: 0x0002A588
		private void DisableWeightInteraction()
		{
            /*
			Singleton<CurrencyBowl>.Instance.ActiveWeights.ForEach(delegate(Rigidbody x)
			{
                UnityEngine.Object.Destroy(x.GetComponent<GenericMainInputInteractable>());
			});
            */
		}

		// Token: 0x06000C06 RID: 3078 RVA: 0x0002C3B8 File Offset: 0x0002A5B8
		private void OrganizeWeights()
		{
            /*
			int num = 5;
			List<Rigidbody> activeWeights = Singleton<CurrencyBowl>.Instance.ActiveWeights;
			for (int i = 0; i < activeWeights.Count; i++)
			{
				int num2 = Mathf.FloorToInt((float)i / (float)num);
				int num3 = i % num;
				Vector3 endValue = this.weightOrganizeAnchor.position + Vector3.left * (float)num2 * 0.6f + Vector3.back * (float)num3 * 0.3f + Random.onUnitSphere * 0.05f;
				Rigidbody token = activeWeights[i];
				token.GetComponent<Collider>().enabled = false;
				Tween.Position(token.transform, endValue, 0.15f, 0f, Tween.EaseInOut, Tween.LoopType.None, null, delegate()
				{
					token.GetComponent<Collider>().enabled = true;
				}, true);
			}
            */
			this.DisableWeightInteraction();
		}

		// Token: 0x06000C07 RID: 3079 RVA: 0x0002C4B0 File Offset: 0x0002A6B0
		private void ShowWeightsHighlighted(int amount)
		{
            /*
			if (Singleton<CurrencyBowl>.Instance.ActiveWeights.Count >= amount)
			{
				for (int i = Singleton<CurrencyBowl>.Instance.ActiveWeights.Count - amount; i < Singleton<CurrencyBowl>.Instance.ActiveWeights.Count; i++)
				{
					Rigidbody rigidbody = Singleton<CurrencyBowl>.Instance.ActiveWeights[i];
					rigidbody.GetComponent<Collider>().enabled = false;
					rigidbody.isKinematic = true;
					Vector3 position = rigidbody.transform.position;
					position.y = this.weightOrganizeAnchor.position.y + 0.1f;
					Tween.Position(rigidbody.transform, position, 0.1f, 0f, null, Tween.LoopType.None, null, null, true);
				}
			}
            */
		}

		// Token: 0x06000C08 RID: 3080 RVA: 0x0002C563 File Offset: 0x0002A763
		private void StopWeightsHighlighted()
		{
            /*
			Singleton<CurrencyBowl>.Instance.ActiveWeights.ForEach(delegate(Rigidbody x)
			{
				Tween.Stop(x.transform.GetInstanceID());
				x.isKinematic = false;
				x.GetComponent<Collider>().enabled = true;
			});
            */
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

        private CardPile _purchasedPile;
		private CardPile purchasedPile
        {
            get 
            {
                if (_purchasedPile == null)
                    _purchasedPile = GetCopiedField<CardPile>("purchasedPile");

                return _purchasedPile;
            }
        }

        private GameObject _weightsBlocker;
		private GameObject weightsBlocker
        {
            get 
            {
                if (_weightsBlocker == null)
                    _weightsBlocker = GetCopiedField<GameObject>("weightsBlocker");

                return _weightsBlocker;
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

		private readonly Vector3 UPGRADES_ANCHOR = new Vector3(-0.6f, 5.01f, -0.55f);

		private readonly Vector3 UPGRADE_SPACING = new Vector3(1.6f, 0f, 0f);

		private List<SelectableCard> upgradesForSale;

        private List<int> upgradePrices;

		private List<SelectableCard> purchasedUpgrades = new List<SelectableCard>();
    }
}