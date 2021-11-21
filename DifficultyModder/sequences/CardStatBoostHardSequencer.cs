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
using Infiniscryption.Core.Helpers;

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

			// Okay, all the visual setup is done
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

			// We're forcing the confirm stone to be active and enabled.
			// You should be able to quit without doing anything.
			this.confirmStone.Enter();

			yield return this.confirmStone.WaitUntilConfirmation();
			CardInfo destroyedCard = null;
			bool finishedBuffing = false;
			int numBuffsGiven = 0;

			if (this.selectionSlot.Card == null)
			{
				// THey've chosen to do nothing!s
				yield return TextDisplayer.Instance.PlayDialogueEvent("StartLeavingWithoutBoosting", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
				ViewManager.Instance.SwitchToView(View.Default, false, false);
				yield return TextDisplayer.Instance.PlayDialogueEvent("LeftWithoutBoosting", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
			}
			else
			{
				while (!finishedBuffing && destroyedCard == null)
				{
					// Change it so there is a chance of failure on the first go
					// No freebies anymore.
					// Well, there will be guaranteed success on the very first one.
					// After that, the base chance goes up.
					if (!RunState.Run.survivorsDead)
					{
						float target = 1f - (float)numBuffsGiven * 0.225f - RunStateHelper.GetFloat("SurvivorBaseChance");
						if (SeededRandom.Value(SaveManager.SaveFile.GetCurrentRandomSeed()) > target)
						{
							destroyedCard = this.selectionSlot.Card.Info;
							this.selectionSlot.Card.Anim.PlayDeathAnimation(true);
							RunState.Run.playerDeck.RemoveCard(this.selectionSlot.Card.Info);
							yield return new WaitForSeconds(1f);
							break; // Break out of the while loop.
						}
					}

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
					if (!this.confirmStone.SelectionConfirmed)
					{
						finishedBuffing = true;
					}
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

				if (!destroyedCard.HasTrait(Trait.KillsSurvivors))
				{

					// Here's where things get worse for you.
					// Unless you killed them
					yield return TextDisplayer.Instance.PlayDialogueEvent("StillHungry", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
					bool punished = false;

					// First, they try to eat a card.
					foreach (Item consumable in ItemsManager.Instance.Consumables)
					{
						// Is this a card bottle item?
						if (consumable is CardBottleItem)
						{
							// Check to see what's in it:
							CardInfo bottleCard = Traverse.Create(consumable as CardBottleItem).Field("cardInfo").GetValue<CardInfo>();
							if (bottleCard.Sacrificable)
							{
								View currentView = ViewManager.Instance.CurrentView;
								ViewManager.Instance.SwitchToView(View.Consumables);
								(consumable as ConsumableItem).PlayShakeAnimation();
								yield return TextDisplayer.Instance.PlayDialogueEvent("EatConsumable", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
								ItemsManager.Instance.DestroyItem((consumable as ConsumableItem).Data.name);
								punished = true;
								ViewManager.Instance.SwitchToView(currentView);
								break;
							}
						}
					}

					// Next, they try to take your pliers (if you have any)
					if (!punished)
					{
						foreach (Item consumable in ItemsManager.Instance.Consumables)
						{
							// Is this a card bottle item?
							if (consumable is PliersItem)
							{
								View currentView = ViewManager.Instance.CurrentView;
								ViewManager.Instance.SwitchToView(View.Consumables);
								(consumable as ConsumableItem).PlayShakeAnimation();
								yield return TextDisplayer.Instance.PlayDialogueEvent("UseWeapon", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
								ViewManager.Instance.SwitchToView(View.Default);
								yield return PliersHelper();
								ItemsManager.Instance.DestroyItem((consumable as ConsumableItem).Data.name);
								punished = true;
								ViewManager.Instance.SwitchToView(currentView);
								break;
							}
						}
					}

					// Next, they try to take your knife (if you have any)
					if (!punished)
					{
						foreach (Item consumable in ItemsManager.Instance.Consumables)
						{
							// Is this a card bottle item?
							if (consumable is SpecialDaggerItem)
							{
								View currentView = ViewManager.Instance.CurrentView;
								ViewManager.Instance.SwitchToView(View.Consumables);
								(consumable as ConsumableItem).PlayShakeAnimation();
								yield return TextDisplayer.Instance.PlayDialogueEvent("UseWeapon", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
								ViewManager.Instance.SwitchToView(View.Default);
								yield return (consumable as SpecialDaggerItem).ActivateSequence();
								ItemsManager.Instance.DestroyItem((consumable as ConsumableItem).Data.name);
								punished = true;
								ViewManager.Instance.SwitchToView(currentView);
								break;
							}
						}
					}

					// Okay, they can't take your bottle, pliers, or dagger.
					// Did you get away with it?
					// Looks like it. 
					// But the base chance won't go back down to 0.
					if (!punished)
					{
						yield return TextDisplayer.Instance.PlayDialogueEvent("NothingTheyWant", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
						RunStateHelper.SetValue("SurvivorBaseChance", "0.15");
					}
					else
					{
						yield return TextDisplayer.Instance.PlayDialogueEvent("RunScared", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
						RunStateHelper.SetValue("SurvivorBaseChance", "0.0");
					}
				}
			}
			else if (this.selectionSlot.Card != null)
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

				// The survivor base chance goes up by 10%
				RunStateHelper.SetValue("SurvivorBaseChance", (RunStateHelper.GetFloat("SurvivorBaseChance") + 0.1f).ToString());
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
			if (destroyedCard != null && destroyedCard.HasTrait(Trait.KillsSurvivors))
			{
				RunState.Run.survivorsDead = true;
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
			this.confirmStone.Enter();
		}

		private static IEnumerator PliersHelper()
        {
            // This all comes from the pliers
            // I'm not completing reusing the method because it does things I don't want it to do.
            UIManager.Instance.Effects.GetEffect<EyelidMaskEffect>().SetIntensity(0.5f, 0.75f);
            yield return new WaitForSeconds(0.5f);
            FirstPersonController.Instance.AnimController.PlayOneShotAnimation("PliersAnimation", null);
            AudioController.Instance.PlaySound2D("whoosh2", MixerGroup.None, 1f, 0.4f, null, null, null, null, false);
            yield return new WaitForSeconds(0.35f);
            AudioController.Instance.PlaySound2D("consumable_pliers_use", MixerGroup.None, 1f, 0f, null, null, null, null, false);
            yield return new WaitForSeconds(0.75f);
            AudioController.Instance.FadeBGMMixerParam("BGMLowpassFreq", 50f, 0.1f);
            UIManager.Instance.Effects.GetEffect<EyelidMaskEffect>().SetIntensity(0f, 0.025f);
            CameraEffects.Instance.Shake(0.1f, 0.5f);
            UIManager.Instance.Effects.GetEffect<ScreenColorEffect>().SetColor(GameColors.Instance.red);
            UIManager.Instance.Effects.GetEffect<ScreenColorEffect>().SetIntensity(1f, 50f);
            CameraEffects.Instance.TweenBlur(4f, 0.03f);
            yield return new WaitForSeconds(0.03f);
            UIManager.Instance.Effects.GetEffect<ScreenColorEffect>().SetIntensity(0f, 0.2f);
            CameraEffects.Instance.TweenBlur(0f, 4f);
            yield return new WaitForSeconds(1f);
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