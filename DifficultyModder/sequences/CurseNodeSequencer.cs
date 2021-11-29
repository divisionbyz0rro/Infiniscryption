using System;
using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using Infiniscryption.Core.Helpers;
using Pixelplacement;
using UnityEngine;
using HarmonyLib;
using Sirenix.OdinInspector;
using TMPro;
using Infiniscryption.Curses.Helpers;

namespace Infiniscryption.Curses.Sequences
{
    public class CurseNodeSequencer : ManagedBehaviour
	{
		// This is the selection screen for which boons are turned on and which views are turned off
        
        private static Traverse _parentContainer;
        private static T GetCopiedField<T>(string fieldName) where T : class
        {
            if (_parentContainer == null)
                _parentContainer = Traverse.Create(Traverse.Create(SpecialNodeHandler.Instance).Field("buyPeltsSequencer").GetValue() as BuyPeltsSequencer);

            return _parentContainer.Field(fieldName).GetValue() as T;
        }

		private List<SelectableCard> availableCurses;
        private List<CurseBase> installedCurses;

		public IEnumerator PlayCurseShop()
		{
			Singleton<ViewManager>.Instance.SwitchToView(View.Default, false, true);
			yield return new WaitForSeconds(0.15f);

            // Set the mood
            TableVisualEffectsManager.Instance.ChangeTableColors(
                GameColors.Instance.brightRed,        // main light color
                GameColors.Instance.darkRed,      // card light color
                GameColors.Instance.glowRed,    // interactables color
                GameColors.Instance.brownOrange,                        // slot default color
                GameColors.Instance.brownOrange,    // slot interactable color
                GameColors.Instance.brownOrange,    // slot highlighted color
                GameColors.Instance.brownOrange,           // queue slot default color
                GameColors.Instance.brownOrange,           // queue slot interactable color
                GameColors.Instance.brownOrange);     // queue slot highlighted color

            // Spawn some trees
            this._forestScenery = GameObject.Instantiate<GameObject>(ResourceBank.Get<GameObject>("Prefabs/Environment/TableEffects/ForestTableEffects"));
            yield return new WaitForSeconds(0.5f);
            AudioController.Instance.PlaySound2D("prospector_trees_enter", MixerGroup.TableObjectsSFX, 0.2f, 0f, null, null, null, null, false);
            yield return new WaitForSeconds(0.25f);            

            LeshyAnimationController.Instance.PutOnMask(LeshyAnimationController.Mask.Woodcarver, true);
            yield return new WaitForSeconds(1.5f);

            InfiniscryptionCursePlugin.Log.LogInfo($"Leshy is ready to give us curses");

			yield return Singleton<TextDisplayer>.Instance.PlayDialogueEvent("CurseIntroIntro", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);

            LeshyAnimationController.Instance.LeftArm.PlayAnimation("doctor_hand_intro");

            yield return Singleton<TextDisplayer>.Instance.PlayDialogueEvent("SummonCurses", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);

            InfiniscryptionCursePlugin.Log.LogInfo($"Showing curses...");
			
            // We will deal out the boons in rows of four.
            // But we have to choose where the cards go based on how many there are.
			availableCurses = new List<SelectableCard>();
            installedCurses = CurseManager.GetAllCurses();
            
			for (int i = 0; i < installedCurses.Count; i++)
			{
				this.CreateCurseCard(i, 0f);
				yield return new WaitForSeconds(0.1f);
			}

            // Generate the confirmstone button
            this._confirmStoneButton = GameObject.Instantiate<GameObject>(ResourceBank.Get<GameObject>("Prefabs/SpecialNodeSequences/ConfirmStoneButton"));
            this._confirmStoneButton.transform.localPosition += new Vector3(3f, 0f, -1f);

            foreach (Component comp in _confirmStoneButton.GetComponentsInChildren<Component>())
            {
                if (comp is MeshRenderer && comp.name == "Quad")
                    (comp as MeshRenderer).material.mainTexture = AssetHelper.LoadTexture("confirm_curse_button");
            }

            ConfirmStoneButton btn = _confirmStoneButton.GetComponentInChildren<ConfirmStoneButton>();
            btn.HighlightCursorType = CursorType.Slap;
            btn.SetColors(
                new Color(0.007843138f, 0.03921569f, 0.06666667f), // default color
                GameColors.Instance.darkRed, // interactable color
                GameColors.Instance.glowRed // hover color
            );
            btn.SetButtonInteractable();

            yield return new WaitForSeconds(0.25f);


            // Look at the view list
			Singleton<ViewManager>.Instance.SwitchToView(View.TradingTopDown, false, false);
			yield return new WaitForSeconds(0.3f);

            TableRuleBook.Instance.SetOnBoard(true);

            yield return Singleton<TextDisplayer>.Instance.PlayDialogueEvent("HowToSelect", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);

            if (!CurseManager.HasSeenCurseSelectBefore)
            {
                yield return Singleton<TextDisplayer>.Instance.PlayDialogueEvent("WhatAreCurses", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                CurseManager.HasSeenCurseSelectBefore = true;
            }

			Singleton<ViewManager>.Instance.Controller.LockState = ViewLockState.Unlocked;
            Singleton<ViewManager>.Instance.Controller.SwitchToControlMode(ViewController.ControlMode.Trading, false);
			
            // Wait until you click the button
			yield return btn.WaitUntilConfirmation();

			Singleton<ViewManager>.Instance.SwitchToView(View.Default, false, true);

			// Destroy the purchaseable cards
			foreach (SelectableCard selectableCard in this.availableCurses)
			{
				int num2 = this.availableCurses.IndexOf(selectableCard);
				Tween.LocalPosition(selectableCard.transform, selectableCard.transform.localPosition + Vector3.forward * 6f, 0.2f, 0.05f * (float)num2, Tween.EaseIn, Tween.LoopType.None, null, null, true);
				selectableCard.SetEnabled(false);
				UnityEngine.Object.Destroy(selectableCard.gameObject, 0.35f);
			}

			Singleton<ViewManager>.Instance.SwitchToView(View.Default, false, false);
			yield return new WaitForSeconds(0.1f);
            yield return Singleton<TextDisplayer>.Instance.PlayDialogueEvent("CursesSelect", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
			yield return LeshyAnimationController.Instance.TakeOffMask();
            
            TableVisualEffectsManager.Instance.ResetTableColors();

            TableRuleBook.Instance.SetOnBoard(false);

            GameObject.Destroy(this._forestScenery);
            GameObject.Destroy(this._confirmStoneButton);

            yield return new WaitForSeconds(1f);
			Singleton<ViewManager>.Instance.Controller.LockState = ViewLockState.Unlocked;

            CurseManager.ResetAll();

			if (Singleton<GameFlowManager>.Instance != null)
			{
                // And we go back to 3D! Hopefully we're still pointing at the skull!
				Singleton<GameFlowManager>.Instance.TransitionToGameState(GameState.Map, null);
				//OpponentAnimationController.Instance.SetExplorationMode(true);
			}

			SaveManager.SaveToFile(false);

			yield break;
        }

        private Vector3 GetLocationForCard(int index)
        {
            int totalCards = installedCurses.Count;
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
		private void CreateCurseCard(int index, float tweenDelay = 0f)
		{
			GameObject newUpgradeCard = UnityEngine.Object.Instantiate<GameObject>(this.selectableCardPrefab, this.transform);
			SelectableCard component = newUpgradeCard.GetComponent<SelectableCard>();
			component.gameObject.SetActive(true);

            InfiniscryptionCursePlugin.Log.LogInfo($"Starting to create card");

            // Need to create a decal card now
            CardInfo curseCard = new CardInfo {
                TempDecals = { installedCurses[index].CurseBackground }
            };
            InfiniscryptionCursePlugin.Log.LogInfo($"Adding curse{installedCurses[index].Title} to info; curse is Active? {installedCurses[index].Active}");
            CardExtensions.SetCurse(curseCard, installedCurses[index]);     

            InfiniscryptionCursePlugin.Log.LogInfo($"Adding info to card");
			component.SetInfo(curseCard);     

            // We need to flip the card over
            component.SetFaceDown(!installedCurses[index].Active, true);
            FixRotations(component);

            InfiniscryptionCursePlugin.Log.LogInfo($"Calculating card location");  

			Vector3 vector = GetLocationForCard(index);
			newUpgradeCard.transform.localPosition = vector + Vector3.forward * 6f;
			newUpgradeCard.transform.localScale = Vector3.Scale(newUpgradeCard.transform.localScale, new Vector3(0.8f, 0.8f, 1f));

			Tween.LocalPosition(newUpgradeCard.transform, vector, 0.2f, tweenDelay, Tween.EaseOut, Tween.LoopType.None, null, null, true);
			Tween.Rotate(newUpgradeCard.transform, new Vector3(0f, 0f, -2f + UnityEngine.Random.value * 4f), Space.Self, 0.25f, tweenDelay, Tween.EaseOut, Tween.LoopType.None, null, null, true);
			CustomCoroutine.WaitThenExecute(tweenDelay, new Action(component.Anim.PlayRiffleSound), false);

            InfiniscryptionCursePlugin.Log.LogInfo($"The card is on the table");

			component.CursorSelectStarted = (Action<MainInputInteractable>)Delegate.Combine(component.CursorSelectStarted, new Action<MainInputInteractable>(delegate(MainInputInteractable c)
			{
				this.FlipCard(c as SelectableCard, index);
			}));

            availableCurses.Add(component);

            InfiniscryptionCursePlugin.Log.LogInfo($"Done");
		}

        private void FlipCard(SelectableCard card, int curseIndex)
        {
            if (card.FaceDown)
            {
                card.SetFaceDown(false, false);
                installedCurses[curseIndex].Active = true;
            } else {
                card.SetFaceDown(true, false);
                installedCurses[curseIndex].Active = false;
            }
            foreach (SelectableCard selectableCard in this.availableCurses)
            {
                selectableCard.SetLocalRotation(0f, 40f, false);
            }
            FixRotations(card);
        }
        
        private void FixRotations(SelectableCard card)
        {
            if (card.FaceDown)
            {
                card.StatsLayer.Renderer.materials[1].SetTexture("_MainTex", CardExtensions.GetCurse(card.Info).CurseCardBack);
            }

            card.SetLocalRotation(card.FaceDown ? 3f : -3f, 20f, false);

	        card.Anim.PlayRiffleSound();
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
		private readonly Vector3 SCALE_FACTOR = new Vector3(1f, 1f, 1f);

        private readonly Vector3 BASE_ANCHOR = new Vector3(-2.8f, 5.01f, -0.12f);

        private readonly Vector3 ROW_OFFSET = new Vector3(0f, 0f, -1.6f);

        private readonly Vector3 COL_OFFSET = new Vector3(1.6f, 0f, 0f);

        private GameObject _forestScenery;

        private GameObject _confirmStoneButton;
    }
}