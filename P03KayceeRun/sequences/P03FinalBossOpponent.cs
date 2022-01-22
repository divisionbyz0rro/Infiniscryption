using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using Infiniscryption.Core.Helpers;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Card;
using InscryptionAPI.Guid;
using UnityEngine;
using System.Linq;
using Pixelplacement;
using HarmonyLib;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    [HarmonyPatch]
    public class P03AscensionOpponent : Part3BossOpponent
    {
        public static Opponent.Type ID = GuidManager.GetEnumValue<Opponent.Type>(P03Plugin.PluginGuid, "P03AscensionFinalBoss");

        public override string PreIntroDialogueId => "";

        public override string PostDefeatedDialogueId => "P03AscensionDefeated";

        private List<string> TributesRemaining = new() { "Luna", "Aaron", "AnthonyPython", "Apples", "Arakulele", "BobTheNerd", "Cyantist", "Eri", "Gareth", "IngoH", "Matzie", "Memez4Life", "TeamDoodz", "VoidSlime"}; 
        private List<string> TributesCreated = new();

        public override IEnumerator PreDefeatedSequence()
        {
            return base.PreDefeatedSequence();
        }

        private CardInfo GenerateTribute(int statPoints)
        {
            if (TributesRemaining.Count == 0)
            {
                TributesRemaining.AddRange(TributesCreated);
                TributesCreated.Clear();
                foreach (CardSlot slot in BoardManager.Instance.OpponentSlotsCopy)
                {
                    if (slot.Card != null && TributesRemaining.Contains(slot.Card.Info.DisplayedNameEnglish))
                    {
                        TributesCreated.Add(slot.Card.Info.DisplayedNameEnglish);
                        TributesRemaining.Remove(slot.Card.Info.DisplayedNameEnglish);
                    }
                }
            }

            CardInfo cardByName = CardLoader.GetCardByName("TRIBUTE");

            int seed = P03AscensionSaveData.RandomSeed + 100 * TurnManager.Instance.TurnNumber;
            int index = SeededRandom.Range(0, TributesRemaining.Count, seed++);

            cardByName.SetAltPortrait(AssetHelper.LoadTexture(TributesRemaining[index].ToLowerInvariant()), UnityEngine.FilterMode.Trilinear);
            
            List<AbilityInfo> validAbilities = ScriptableObjectLoader<AbilityInfo>.AllData.FindAll((AbilityInfo x) => x.metaCategories.Contains(AbilityMetaCategory.BountyHunter));
            CardModificationInfo cardModificationInfo = CardInfoGenerator.CreateRandomizedAbilitiesStatsMod(validAbilities, statPoints, 1, 1);
            cardModificationInfo.nameReplacement = Localization.Translate(TributesRemaining[index]);
            cardModificationInfo.energyCostAdjustment = statPoints / 2;
            cardByName.Mods.Add(cardModificationInfo);

            TributesCreated.Add(TributesRemaining[index]);
            TributesRemaining.Remove(TributesRemaining[index]);

            return cardByName;
        }

        public override IEnumerator StartBattleSequence()
        {
           	yield return new WaitForSeconds(1f);
            yield return TextDisplayer.Instance.PlayDialogueEvent("P03IntroductionToModding", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            ViewManager.Instance.SwitchToView(View.P03FaceClose, false, false);
            yield return TextDisplayer.Instance.PlayDialogueEvent("P03IntroductionClose", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            ViewManager.Instance.SwitchToView(View.BoardCentered, false, false);

            yield return BoardManager.Instance.CreateCardInSlot(CardLoader.GetCardByName("KOPIE"), BoardManager.Instance.OpponentSlotsCopy[2]);
            yield return new WaitForSeconds(0.15f);

            yield return TextDisplayer.Instance.PlayDialogueEvent("P03PhaseOneKopie", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            
            yield return new WaitForSeconds(0.45f);
            ViewManager.Instance.SwitchToView(View.Default, false, false);
        }

        public override IEnumerator StartNewPhaseSequence()
        {
           	yield return new WaitForSeconds(1f);
            yield return TextDisplayer.Instance.PlayDialogueEvent("P03PhaseTwoMADH", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            ViewManager.Instance.SwitchToView(View.BoardCentered, false, false);

            foreach (int idx in new int[] { 0, 4 })
            {
                if (BoardManager.Instance.opponentSlots[idx].Card != null)
                {
                    yield return BoardManager.Instance.opponentSlots[idx].Card.Die(true);
                    yield return new WaitForSeconds(0.25f);
                }
            }

            yield return BoardManager.Instance.CreateCardInSlot(CardLoader.GetCardByName("MADH95_LEFT"), BoardManager.Instance.OpponentSlotsCopy[0]);
            yield return new WaitForSeconds(0.15f);
            yield return BoardManager.Instance.CreateCardInSlot(CardLoader.GetCardByName("MADH95_RIGHT"), BoardManager.Instance.OpponentSlotsCopy[4]);
            yield return new WaitForSeconds(0.15f);

            yield return TextDisplayer.Instance.PlayDialogueEvent("P03PhaseTwoMADH2", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            
            yield return new WaitForSeconds(0.45f);
            ViewManager.Instance.SwitchToView(View.Default, false, false);
        
        }

        private static readonly int[] MODDERS_PART_1 = new int[] { 0, 1, 1, 2, 0, 1, 1, 2, 0, 2};

        private static readonly int[] MODDERS_PART_2 = new int[] { 1, 2, 1, 1, 2, 0, 1, 2, 1};

        public override IEnumerator QueueNewCards(bool doTween = true, bool changeView = true)
        {
            List<CardSlot> slotsToQueue = BoardManager.Instance.OpponentSlotsCopy.FindAll((CardSlot x) => x.Card == null || (x.Card != null && !x.Card.Info.HasTrait(Trait.Terrain)));
            slotsToQueue.RemoveAll((CardSlot x) => base.Queue.Exists((PlayableCard y) => y.QueuedSlot == x));
            int numCardsToQueue = 0;
            int[] plan = (base.NumLives > 1) ? MODDERS_PART_1 : MODDERS_PART_2;
            if (TurnManager.Instance.TurnNumber < plan.Length)
                numCardsToQueue = plan[TurnManager.Instance.TurnNumber];
            
            for (int i = 0; i < numCardsToQueue; i++)
            {
                if (slotsToQueue.Count > 0)
                {
                    int statPoints = Mathf.RoundToInt((float)Mathf.Min(6, TurnManager.Instance.TurnNumber + 1) * 2.5f);
                    CardSlot slot = slotsToQueue[Random.Range(0, slotsToQueue.Count)];
                    CardInfo card = GenerateTribute(statPoints);
                    if (card != null)
                    {
                        yield return base.QueueCard(card, slot, doTween, changeView, true);
                        slotsToQueue.Remove(slot);
                    }
                }
            }
            yield break;
        }

        public IEnumerator ShopForModSequence(string modName, bool shopping = true, bool repeat = false)
        {
            ViewManager.Instance.SwitchToView(View.P03Face, false, false);
            
            if (shopping)
            {
                yield return TextDisplayer.Instance.PlayDialogueEvent("P03ShoppingForMod", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                yield return new WaitForSeconds(0.3f);
            }
            if (repeat)
                yield return TextDisplayer.Instance.PlayDialogueEvent("P03ReplayMod", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, new string[] { modName }, null);
            else
                yield return TextDisplayer.Instance.PlayDialogueEvent("P03SelectedMod", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, new string[] { modName }, null);
            P03AnimationController.Instance.SwitchToFace(P03AnimationController.Face.Thinking);
            yield return new WaitForSeconds(0.3f);
            ViewManager.Instance.SwitchToView(View.Default, false, false);
        }

        private bool scalesHidden = false;

        [HarmonyPatch(typeof(Scales3D), nameof(Scales3D.AddDamage))]
        [HarmonyPostfix]
        public static IEnumerator DontShowDamageWhenScalesHidden(IEnumerator sequence)
        {
            if (TurnManager.Instance != null &&
                TurnManager.Instance.Opponent is P03AscensionOpponent &&
                (TurnManager.Instance.Opponent as P03AscensionOpponent).scalesHidden)
                yield break;

            yield return sequence;
        }

        public IEnumerator UnityEngineSequence()
        {
            ViewManager.Instance.SwitchToView(View.P03Face, false, false);
            yield return TextDisplayer.Instance.PlayDialogueEvent("P03UnityMod", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            ViewManager.Instance.SwitchToView(View.Consumables, false, false);
            yield return new WaitForSeconds(0.5f);
            ResourceDrone.Instance.gameObject.transform.localPosition = ResourceDrone.Instance.gameObject.transform.localPosition + Vector3.up * 6f;
            yield return new WaitForSeconds(0.5f);
            ViewManager.Instance.SwitchToView(View.Scales, false, false);
            yield return new WaitForSeconds(0.5f);
            foreach(Renderer rend in LifeManager.Instance.Scales3D.gameObject.GetComponentsInChildren<Renderer>())
                rend.enabled = false;

            scalesHidden = true;
            yield return new WaitForSeconds(0.5f);
            ViewManager.Instance.SwitchToView(View.P03Face, false, false);
            yield return TextDisplayer.Instance.PlayDialogueEvent("P03UnityModDone", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
            ViewManager.Instance.SwitchToView(View.Default);
        }

        public IEnumerator APISequence()
        {
            ViewManager.Instance.SwitchToView(View.P03Face, false, false);
            yield return TextDisplayer.Instance.PlayDialogueEvent("P03ApiInstalled", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);            
            ViewManager.Instance.SwitchToView(View.Consumables, false, false);
            InteractionCursor.Instance.InteractionDisabled = true;
            yield return new WaitForSeconds(0.2f);
            yield return ResourcesManager.Instance.RefreshEnergy();
            yield return new WaitForSeconds(0.6f);
            int maxEnergy = ResourcesManager.Instance.PlayerMaxEnergy;
            Traverse resourceTrav = Traverse.Create(ResourcesManager.Instance).Property("PlayerMaxEnergy");

            while (maxEnergy > 3)
            {
                yield return ResourcesManager.Instance.SpendEnergy(ResourcesManager.Instance.PlayerEnergy);
                maxEnergy -= 1;
                resourceTrav.SetValue(maxEnergy);
                yield return ResourcesManager.Instance.RefreshEnergy();
                yield return new WaitForSeconds(0.6f);
            }

            InteractionCursor.Instance.InteractionDisabled = false;
            ViewManager.Instance.SwitchToView(View.Default);
        }

        public IEnumerator DraftSequence()
        {
            ViewManager.Instance.SwitchToView(View.BoardCentered, false, false);
            for (int i = 0; i < 2; i++)
            {
                IEnumerable<CardSlot> slots = BoardManager.Instance.OpponentSlotsCopy.Where(c => c != null && c.Card == null);
                CardSlot slot = i == 0 ? slots.FirstOrDefault() : slots.LastOrDefault();
                if (slot == null)
                    continue;

                CardInfo draftToken = CardLoader.GetCardByName(CustomCards.DRAFT_TOKEN);
                yield return BoardManager.Instance.CreateCardInSlot(draftToken, slot);
                yield return new WaitForSeconds(0.55f);
            }
            yield return new WaitForSeconds(1f);
            ViewManager.Instance.SwitchToView(View.Default, false, false);
        }

        public IEnumerator ExchangeTokensSequence()
        {
            List<CardSlot> tokenSlots = BoardManager.Instance.OpponentSlotsCopy.Where(c => c != null && c.Card != null && c.Card.Info.name == CustomCards.DRAFT_TOKEN).ToList();

            ViewManager.Instance.SwitchToView(View.P03Face, false, false);
            if (tokenSlots.Count == 0)
            {
                yield return TextDisplayer.Instance.PlayDialogueEvent("P03NoTokens", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                yield break;
            }

            yield return TextDisplayer.Instance.PlayDialogueEvent("P03Drafting", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);

            if (PlayerHand.Instance.cardsInHand.Count == 0)
            {
                yield return TextDisplayer.Instance.PlayDialogueEvent("P03NoCardsInHand", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                yield break;
            }

            ViewManager.Instance.SwitchToView(View.BoardCentered, false, false);
            foreach (CardSlot slot in tokenSlots)
            {
                slot.Card.ExitBoard(0.4f, Vector3.zero);
                yield return new WaitForSeconds(0.4f);
            }

            InteractionCursor.Instance.InteractionDisabled = true;

            int seed = P03AscensionSaveData.RandomSeed + 10 * TurnManager.Instance.TurnNumber;
            foreach (CardSlot slot in tokenSlots)
            {
                ViewManager.Instance.SwitchToView(View.Hand, false, false);
                foreach (PlayableCard card in PlayerHand.Instance.CardsInHand)
                {
                    PlayerHand.Instance.OnCardInspected(card);
                    yield return new WaitForSeconds(0.33f);
                }
                List<PlayableCard> possibles = PlayerHand.Instance.CardsInHand.Where(c => c.Info.name != CustomCards.DRAFT_TOKEN).ToList();
                PlayableCard cardToSteal = possibles[SeededRandom.Range(0, possibles.Count, seed++)];
                PlayerHand.Instance.OnCardInspected(cardToSteal);
                yield return new WaitForSeconds(0.75f);

                PlayerHand.Instance.RemoveCardFromHand(cardToSteal);
                cardToSteal.SetEnabled(false);
				cardToSteal.Anim.SetTrigger("fly_off");
				Tween.Position(cardToSteal.transform, cardToSteal.transform.position + new Vector3(0f, 3f, 5f), 0.4f, 0f, Tween.EaseInOut, Tween.LoopType.None, null, delegate()
				{
					Object.Destroy(cardToSteal.gameObject);
				}, true);
                yield return new WaitForSeconds(0.75f);

                CardInfo draftToken = CardLoader.GetCardByName(CustomCards.DRAFT_TOKEN);
                draftToken.mods.Add(new (Ability.DrawRandomCardOnDeath));
                PlayableCard tokenCard = CardSpawner.SpawnPlayableCard(draftToken);
                yield return PlayerHand.Instance.AddCardToHand(tokenCard, Vector3.zero, 0f);
                yield return new WaitForSeconds(0.6f);

                ViewManager.Instance.SwitchToView(View.BoardCentered, false, false);
                yield return BoardManager.Instance.CreateCardInSlot(cardToSteal.Info, slot);
                yield return new WaitForSeconds(0.65f);
            }

            ViewManager.Instance.SwitchToView(View.Default);
            InteractionCursor.Instance.InteractionDisabled = false;
            yield break;
        }

        public IEnumerator HammerSequence()
        {
            List<CardSlot> slots = BoardManager.Instance.PlayerSlotsCopy.Where(s => s != null && s.Card != null).ToList();

            if (slots.Count == 0)
            {
                ViewManager.Instance.SwitchToView(View.P03Face, false, false);
                yield return TextDisplayer.Instance.PlayDialogueEvent("P03AngryNoHammer", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                ViewManager.Instance.SwitchToView(View.Default);
                yield return new WaitForSeconds(0.1f);
                yield break;
            }

            ViewManager.Instance.SwitchToView(View.P03Face, false, false);
                yield return TextDisplayer.Instance.PlayDialogueEvent("P03HammerModHappy", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);

            int seed = P03AscensionSaveData.RandomSeed + 10 * TurnManager.Instance.TurnNumber + 234;
            CardSlot target = slots[SeededRandom.Range(0, slots.Count, seed)];

            // Find the hammer item
            HammerItem hammer = ItemsManager.Instance.Slots.First(s => s.Item is HammerItem).Item as HammerItem;

            hammer.PlayExitAnimation();
            InteractionCursor.Instance.InteractionDisabled = true;
            yield return new WaitForSeconds(0.1f);
            //UIManager.Instance.Effects.GetEffect<EyelidMaskEffect>().SetIntensity(0.6f, 0.2f);
            ViewManager.Instance.SwitchToView(hammer.SelectionView, false, false);
            yield return new WaitForSeconds(0.1f);
            Transform firstPersonItem = FirstPersonController.Instance.AnimController.SpawnFirstPersonAnimation(hammer.FirstPersonPrefabId, null).transform;
            firstPersonItem.localPosition = hammer.FirstPersonItemPos + Vector3.right * 3f + Vector3.forward * 1f;
            firstPersonItem.localEulerAngles = hammer.FirstPersonItemEulers;
            InteractionCursor.Instance.InteractionDisabled = false;

            foreach (CardSlot slot in BoardManager.Instance.PlayerSlotsCopy.Where(s => s != null && s.Card != null))
            {
                hammer.MoveItemToPosition(firstPersonItem, slot.transform.position);
                yield return new WaitForSeconds(0.5f);
            }

            hammer.MoveItemToPosition(firstPersonItem, target.transform.position);
            yield return new WaitForSeconds(0.25f);

            ViewManager.Instance.Controller.LockState = ViewLockState.Locked;
            yield return hammer.OnValidTargetSelected(target, firstPersonItem.gameObject);
            yield return new WaitForSeconds(0.5f);
            
            GameObject.Destroy(firstPersonItem.gameObject);
            //UIManager.Instance.Effects.GetEffect<EyelidMaskEffect>().SetIntensity(0f, 0.2f);
            ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;
            InteractionCursor.Instance.InteractionDisabled = false;
            yield break;
        }
    }
}