using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using DiskCardGame;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine.UI;

namespace Infiniscryption.DifficultyMod.Sequences
{
	public class CardStatBoostHardSequencer : ManagedBehaviour
	{
		// There isn't really a good way to just modify this sequencer
        // We have to replace it

        private static Traverse _parentContainer;
        private static T GetCopiedField<T>(string fieldName) where T : class
        {
            if (_parentContainer == null)
                _parentContainer = Traverse.Create(Traverse.Create(SpecialNodeHandler.Instance).Field("cardStatBoostSequencer").GetValue() as CardStatBoostSequencer);

            return _parentContainer.Field(fieldName).GetValue() as T;
        }
        
		public IEnumerator StatBoostSequence()
		{
            // Removed logic where the first time you come across the campfire it's an attack mod by defualt
			this.attackMod = SeededRandom.Bool(SaveManager.SaveFile.GetCurrentRandomSeed());
			this.selectionSlot.specificRenderers[0].material.mainTexture = (this.attackMod ? this.attackModSlotTexture : this.healthModSlotTexture);

            // Set up the figures
			this.figurines.ForEach(delegate(CompositeFigurine x)
			{
				x.SetArms(this.attackMod ? CompositeFigurine.FigurineType.Wildling : CompositeFigurine.FigurineType.SettlerWoman);
                x.gameObject.SetActive(false);
			});
			
			this.stakeRingParent.SetActive(false);

            // Set up the fire
			this.campfireLight.gameObject.SetActive(false);
			this.campfireLight.intensity = 0f;
			this.campfireCardLight.intensity = 0f;

            // SElection slot
			this.selectionSlot.Disable();
			this.selectionSlot.gameObject.SetActive(false);
			yield return new WaitForSeconds(0.3f);

			ExplorableAreaManager.Instance.HangingLight.gameObject.SetActive(false);
			ExplorableAreaManager.Instance.HandLight.gameObject.SetActive(false);
			ViewManager.Instance.SwitchToView(View.Default, false, true);
			ViewManager.Instance.OffsetPosition(new Vector3(0f, 0f, 2.25f), 0.1f);
			yield return new WaitForSeconds(1f);

            // Survivors die if they eat a poisonous card.
			if (!RunState.Run.survivorsDead)
			{
				this.figurines.ForEach(delegate(CompositeFigurine x)
				{
					x.gameObject.SetActive(true);
				});
			}
            
			this.stakeRingParent.SetActive(true);
			ExplorableAreaManager.Instance.HandLight.gameObject.SetActive(true);
			this.campfireLight.gameObject.SetActive(true);
			this.selectionSlot.gameObject.SetActive(true);
			this.selectionSlot.RevealAndEnable();
			this.selectionSlot.ClearDelegates();
			SelectCardFromDeckSlot selectCardFromDeckSlot = this.selectionSlot;
			selectCardFromDeckSlot.CursorSelectStarted = (Action<MainInputInteractable>)Delegate.Combine(selectCardFromDeckSlot.CursorSelectStarted, new Action<MainInputInteractable>(this.OnSlotSelected));
			if (UnityEngine.Random.value < 0.25f && VideoCameraRig.Instance != null)
			{
				VideoCameraRig.Instance.PlayCameraAnim("refocus_quick");
			}
			AudioController.Instance.PlaySound3D("campfire_light", MixerGroup.TableObjectsSFX, this.selectionSlot.transform.position, 1f, 0f, null, null, null, null, false);
			AudioController.Instance.SetLoopAndPlay("campfire_loop", 1, true, true);
			AudioController.Instance.SetLoopVolumeImmediate(0f, 1);
			AudioController.Instance.FadeInLoop(0.5f, 0.75f, new int[]
			{
				1
			});
			InteractionCursor.Instance.SetEnabled(false);
			yield return new WaitForSeconds(0.25f);
			yield return this.pile.SpawnCards(RunState.DeckList.Count, 0.5f);
			TableRuleBook.Instance.SetOnBoard(true);
			InteractionCursor.Instance.SetEnabled(true);
			if (RunState.Run.survivorsDead)
			{
				yield return TextDisplayer.Instance.PlayDialogueEvent("StatBoostSurvivorsDead", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
			}
			else
			{
				yield return TextDisplayer.Instance.PlayDialogueEvent("StatBoostIntro", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, new string[]
				{
					this.GetTranslatedStatText(this.attackMod)
				}, null);
			}
			yield return this.confirmStone.WaitUntilConfirmation();
			CardInfo destroyedCard = null;
			bool finishedBuffing = false;
			int numBuffsGiven = 0;
			while (!finishedBuffing && destroyedCard == null)
			{
				int num = numBuffsGiven;
				numBuffsGiven = num + 1;
				this.selectionSlot.Disable();
				RuleBookController.Instance.SetShown(false, true);
				yield return new WaitForSeconds(0.25f);
				AudioController.Instance.PlaySound3D("card_blessing", MixerGroup.TableObjectsSFX, this.selectionSlot.transform.position, 1f, 0f, null, null, null, null, false);
				this.selectionSlot.Card.Anim.PlayTransformAnimation();
				this.ApplyModToCard(this.selectionSlot.Card.Info);
				yield return new WaitForSeconds(0.15f);
				this.selectionSlot.Card.SetInfo(this.selectionSlot.Card.Info);
				this.selectionSlot.Card.SetInteractionEnabled(false);
				yield return new WaitForSeconds(0.75f);
				if (SaveManager.SaveFile.pastRuns.Count >= 4)
				{
					if (numBuffsGiven == 4)
					{
						break;
					}
					if (!RunState.Run.survivorsDead)
					{
						yield return TextDisplayer.Instance.PlayDialogueEvent("StatBoostPushLuck" + numBuffsGiven, TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
						yield return new WaitForSeconds(0.1f);
						switch (numBuffsGiven)
						{
						case 1:
							TextDisplayer.Instance.ShowMessage("Push your luck? Or pull away?", Emotion.Neutral, TextDisplayer.LetterAnimation.WavyJitter, DialogueEvent.Speaker.Single, null);
							break;
						case 2:
							TextDisplayer.Instance.ShowMessage("Push your luck further? Or run back?", Emotion.Neutral, TextDisplayer.LetterAnimation.WavyJitter, DialogueEvent.Speaker.Single, null);
							break;
						case 3:
							TextDisplayer.Instance.ShowMessage("Recklessly continue?", Emotion.Neutral, TextDisplayer.LetterAnimation.WavyJitter, DialogueEvent.Speaker.Single, null);
							break;
						}
					}
					
                    bool cancelledByClickingCard = false;
					this.retrieveCardInteractable.gameObject.SetActive(true);
					this.retrieveCardInteractable.CursorSelectEnded = null;
					GenericMainInputInteractable genericMainInputInteractable = this.retrieveCardInteractable;
					genericMainInputInteractable.CursorSelectEnded = (Action<MainInputInteractable>)Delegate.Combine(genericMainInputInteractable.CursorSelectEnded, new Action<MainInputInteractable>(delegate(MainInputInteractable i)
					{
						cancelledByClickingCard = true;
					}));
					this.confirmStone.Unpress();
					base.StartCoroutine(this.confirmStone.WaitUntilConfirmation());
					yield return new WaitUntil(() => (this.confirmStone.SelectionConfirmed || InputButtons.GetButton(Button.LookDown) || InputButtons.GetButton(Button.Cancel)) || cancelledByClickingCard);
					TextDisplayer.Instance.Clear();
					this.retrieveCardInteractable.gameObject.SetActive(false);
					this.confirmStone.Disable();
					yield return new WaitForSeconds(0.1f);
					if (this.confirmStone.SelectionConfirmed)
					{
						if (!RunState.Run.survivorsDead)
						{
							float num2 = 1f - (float)numBuffsGiven * 0.225f;
							if (SeededRandom.Value(SaveManager.SaveFile.GetCurrentRandomSeed()) > num2)
							{
								destroyedCard = this.selectionSlot.Card.Info;
								this.selectionSlot.Card.Anim.PlayDeathAnimation(true);
								RunState.Run.playerDeck.RemoveCard(this.selectionSlot.Card.Info);
								yield return new WaitForSeconds(1f);
							}
						}
					}
					else
					{
						finishedBuffing = true;
					}
				}
				else
				{
					finishedBuffing = true;
				}
			}
			if (destroyedCard != null)
			{
				yield return TextDisplayer.Instance.PlayDialogueEvent("StatBoostCardEaten", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, new string[]
				{
					this.selectionSlot.Card.Info.DisplayedNameLocalized
				}, null);
				yield return new WaitForSeconds(0.1f);
				this.selectionSlot.DestroyCard();
			}
			else
			{
				if (!RunState.Run.survivorsDead)
				{
					yield return TextDisplayer.Instance.PlayDialogueEvent("StatBoostOutro", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, new string[]
					{
						this.GetTranslatedStatText(this.attackMod),
						this.selectionSlot.Card.Info.DisplayedNameLocalized
					}, null);
				}
				yield return new WaitForSeconds(0.1f);
				this.selectionSlot.FlyOffCard();
			}
			ViewManager.Instance.SwitchToView(View.Default, false, false);
			yield return new WaitForSeconds(0.25f);
			AudioController.Instance.PlaySound3D("campfire_putout", MixerGroup.TableObjectsSFX, this.selectionSlot.transform.position, 1f, 0f, null, null, null, null, false);
			AudioController.Instance.StopLoop(1);
			this.campfireLight.gameObject.SetActive(false);
			ExplorableAreaManager.Instance.HandLight.gameObject.SetActive(false);
			yield return this.pile.DestroyCards(0.5f);
			yield return new WaitForSeconds(0.2f);
			this.figurines.ForEach(delegate(CompositeFigurine x)
			{
				x.gameObject.SetActive(false);
			});
			this.stakeRingParent.SetActive(false);
			this.confirmStone.SetStoneInactive();
			this.selectionSlot.gameObject.SetActive(false);
			CustomCoroutine.WaitThenExecute(0.4f, delegate
			{
				ExplorableAreaManager.Instance.HangingLight.intensity = 0f;
				ExplorableAreaManager.Instance.HangingLight.gameObject.SetActive(true);
				ExplorableAreaManager.Instance.HandLight.intensity = 0f;
				ExplorableAreaManager.Instance.HandLight.gameObject.SetActive(true);
			}, false);
			if (destroyedCard != null)
			{
				if (RunState.Run.consumables.Count < 3)
				{
					yield return new WaitForSeconds(0.4f);
					ViewManager.Instance.SwitchToView(View.Consumables, false, false);
					yield return new WaitForSeconds(0.2f);
					RunState.Run.consumables.Add("PiggyBank");
					ItemsManager.Instance.UpdateItems(false);
					yield return new WaitForSeconds(0.5f);
					yield return TextDisplayer.Instance.PlayDialogueEvent("StatBoostCardEatenBones", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
				}
				if (destroyedCard.HasTrait(Trait.KillsSurvivors))
				{
					RunState.Run.survivorsDead = true;
				}
			}
			ProgressionData.SetMechanicLearned(MechanicsConcept.CardStatBoost);
			if (GameFlowManager.Instance != null)
			{
				GameFlowManager.Instance.TransitionToGameState(GameState.Map, null);
			}
			yield break;
		}

		private void OnSlotSelected(MainInputInteractable slot)
		{
			this.selectionSlot.SetEnabled(false);
			this.selectionSlot.ShowState(HighlightedInteractable.State.NonInteractable, false, 0.15f);
			this.confirmStone.Exit();
			List<CardInfo> validCards = this.GetValidCards();
			(slot as SelectCardFromDeckSlot).SelectFromCards(validCards, new Action(this.OnSelectionEnded), false);
		}

		private void OnSelectionEnded()
		{
			this.selectionSlot.SetShown(true, false);
			this.selectionSlot.ShowState(HighlightedInteractable.State.Interactable, false, 0.15f);
			ViewManager.Instance.SwitchToView(View.Default, false, true);
			if (this.selectionSlot.Card != null)
			{
				this.confirmStone.Enter();
			}
		}

		private List<CardInfo> GetValidCards()
		{
			List<CardInfo> list = new List<CardInfo>(RunState.DeckList);
			list.RemoveAll((CardInfo x) => x.SpecialAbilities.Contains(SpecialTriggeredAbility.RandomCard) || x.traits.Contains(Trait.Pelt) || x.traits.Contains(Trait.Terrain));
			if (this.attackMod)
			{
				list.RemoveAll((CardInfo x) => StatIconInfo.IconAppliesToAttack(x.SpecialStatIcon));
			}
			else
			{
				list.RemoveAll((CardInfo x) => StatIconInfo.IconAppliesToHealth(x.SpecialStatIcon));
			}
			return list;
		}

		private void ApplyModToCard(CardInfo card)
		{
			CardModificationInfo cardModificationInfo = new CardModificationInfo();
			if (this.attackMod)
			{
				cardModificationInfo.attackAdjustment = 1;
			}
			else
			{
				cardModificationInfo.healthAdjustment = 2;
			}
			RunState.Run.playerDeck.ModifyCard(card, cardModificationInfo);
		}

		private string GetTranslatedStatText(bool isAttackMod)
		{
			if (Localization.CurrentLanguage == Language.English)
			{
				if (!isAttackMod)
				{
					return "Health";
				}
				return "Power";
			}
			else
			{
				if (isAttackMod)
				{
					return Localization.TranslateWithID("MISC_036");
				}
				return Localization.TranslateWithID("MISC_493");
			}
		}

        private Texture _attackModSlotTexture;
		private Texture attackModSlotTexture
        {
            get 
            {
                if (_attackModSlotTexture == null)
                    _attackModSlotTexture = GetCopiedField<Texture>("attackModSlotTexture");

                return _attackModSlotTexture;
            }
        }

		private Texture _healthModSlotTexture;
		private Texture healthModSlotTexture
        {
            get 
            {
                if (_healthModSlotTexture == null)
                    _healthModSlotTexture = GetCopiedField<Texture>("healthModSlotTexture");

                return _healthModSlotTexture;
            }
        }

		private Light _campfireLight;
		private Light campfireLight
        {
            get 
            {
                if (_campfireLight == null)
                    _campfireLight = GetCopiedField<Light>("campfireLight");

                return _campfireLight;
            }
        }

		private Light _campfireCardLight;
		private Light campfireCardLight
        {
            get 
            {
                if (_campfireCardLight == null)
                    _campfireCardLight = GetCopiedField<Light>("campfireCardLight");

                return _campfireCardLight;
            }
        }

		private CardPile _pile;
		private CardPile pile
        {
            get 
            {
                if (_pile == null)
                    _pile = GetCopiedField<CardPile>("pile");

                return _pile;
            }
        }

		private SelectCardFromDeckSlot _selectionSlot;
		private SelectCardFromDeckSlot selectionSlot
        {
            get 
            {
                if (_selectionSlot == null)
                    _selectionSlot = GetCopiedField<SelectCardFromDeckSlot>("selectionSlot");

                return _selectionSlot;
            }
        }

		private ConfirmStoneButton _confirmStone;
		private ConfirmStoneButton confirmStone
        {
            get 
            {
                if (_confirmStone == null)
                    _confirmStone = GetCopiedField<ConfirmStoneButton>("confirmStone");

                return _confirmStone;
            }
        }

		private GameObject _stakeRingParent;
		private GameObject stakeRingParent
        {
            get 
            {
                if (_stakeRingParent == null)
                    _stakeRingParent = GetCopiedField<GameObject>("stakeRingParent");

                return _stakeRingParent;
            }
        }

		private List<CompositeFigurine> _figurines;
		private List<CompositeFigurine> figurines
        {
            get 
            {
                if (_figurines == null)
                    _figurines = GetCopiedField<List<CompositeFigurine>>("figurines");

                return _figurines;
            }
        }

		private GenericMainInputInteractable _retrieveCardInteractable;
		private GenericMainInputInteractable retrieveCardInteractable
        {
            get 
            {
                if (_retrieveCardInteractable == null)
                    _retrieveCardInteractable = GetCopiedField<GenericMainInputInteractable>("retrieveCardInteractable");

                return _retrieveCardInteractable;
            }
        }

		private bool attackMod;

		private const int MAX_BUFFS = 3;
	}
}