using System.Collections;
using DiskCardGame;
using System.Linq;
using Infiniscryption.P03KayceeRun.Patchers;
using UnityEngine;
using System.Collections.Generic;
using Infiniscryption.Core.Helpers;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    public class P03FinalBossSequencer : BossBattleSequencer
    {
        public override Opponent.Type BossType => P03AscensionOpponent.ID;

        public override StoryEvent DefeatedStoryEvent => EventManagement.DEFEATED_P03;

        public const string SequenceID = "p03ascensionFinalBossSequence";

        public static readonly string[] MODS = new string[] { "Kopie's Hammer Mod", "Porta's Drafting Mod", "Cyantist's API", "Sinai Unity Explorer" };

        public P03AscensionOpponent P03AscensionOpponent
        {
            get
            {
                return TurnManager.Instance.opponent as P03AscensionOpponent;
            }
        }

        private int upkeepCounter = -1;

        public override IEnumerator OpponentUpkeep()
        {
            upkeepCounter += 1;
            P03AnimationController.Instance.SwitchToFace(P03AnimationController.Face.Default);
            switch (upkeepCounter)
            {
                case 1:
                    yield return P03AscensionOpponent.ShopForModSequence(MODS[0]);
                    yield break;

                case 2:
                    yield return P03AscensionOpponent.HammerSequence();
                    yield break;

                case 3:
                    yield return P03AscensionOpponent.ShopForModSequence(MODS[1], false);
                    yield break;

                case 4:
                    yield return P03AscensionOpponent.DraftSequence();
                    yield break;

                case 5:
                    yield return P03AscensionOpponent.ExchangeTokensSequence();
                    yield break;

                case 6:
                    yield return P03AscensionOpponent.ShopForModSequence(MODS[2], false);
                    yield break;

                case 7:
                    yield return P03AscensionOpponent.APISequence();
                    yield break;

                case 8:
                    yield return P03AscensionOpponent.ShopForModSequence(MODS[3], false);
                    yield break;

                case 9:
                    yield return P03AscensionOpponent.UnityEngineSequence();
                    yield break;

                case 10:
                case 17:
                case 24:
                    yield return P03AscensionOpponent.ShopForModSequence(MODS[0], false, true);
                    yield break;

                case 11:
                case 18:
                case 25:
                    yield return P03AscensionOpponent.HammerSequence();
                    yield break;

                case 12:
                case 19:
                case 26:
                    yield return P03AscensionOpponent.ShopForModSequence(MODS[1], false, true);
                    yield break;

                case 13:
                case 20:
                case 27:
                    yield return P03AscensionOpponent.DraftSequence();
                    yield break;

                case 14:
                case 21:
                case 28:
                    yield return P03AscensionOpponent.ExchangeTokensSequence();
                    yield break;

                case 15:
                case 22:
                case 29:
                    yield return P03AscensionOpponent.ShopForModSequence(MODS[2], false, true);
                    yield break;

                case 16:
                case 23:
                case 30:
                    yield return P03AscensionOpponent.APISequence();
                    yield break;
            }
        }        

        public override IEnumerator GameEnd(bool playerWon)
        {
            if (playerWon)
            {
                ViewManager.Instance.SwitchToView(View.P03Face, false, false);
                yield return TextDisplayer.Instance.PlayDialogueEvent("P03BeatFinalBoss", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                ViewManager.Instance.SwitchToView(View.Default, false, false);
                P03AnimationController.Instance.SwitchToFace(P03AnimationController.Face.Happy, true, true);
                yield return TextDisplayer.Instance.PlayDialogueEvent("P03NothingMatters", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                ViewManager.Instance.SwitchToView(View.DefaultUpwards, false, false);
                yield return TextDisplayer.Instance.PlayDialogueEvent("P03ThreeMovesAhead", TextDisplayer.MessageAdvanceMode.Auto, TextDisplayer.EventIntersectMode.Wait, null, null);
                FactoryScrybes scrybes = FactoryManager.Instance.Scrybes;
                scrybes.Show();
                yield return new WaitForSeconds(0.2f);
                P03AnimationController.Instance.SetHeadTrigger("neck_snap");
                CustomCoroutine.WaitOnConditionThenExecute(() => P03AnimationController.Instance.CurrentFace == P03AnimationController.Face.Choking, delegate
                {
                    AudioController.Instance.PlaySound3D("p03_head_off", MixerGroup.TableObjectsSFX, P03AnimationController.Instance.transform.position, 1f, 0f, null, null, null, null, false);
                });
                yield return new WaitForSeconds(12f);
                P03AnimationController.Instance.gameObject.SetActive(false);
                scrybes.leshy.SetEyesAnimated(true);
                yield return TextDisplayer.Instance.PlayDialogueEvent("LeshyFinalBossDialogue", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                yield return new WaitForSeconds(0.5f);
                EventManagement.FinishAscension(true);
            }
            else
            {
                ViewManager.Instance.SwitchToView(View.P03Face, false, false);
                yield return TextDisplayer.Instance.PlayDialogueEvent("P03LostFinalBoss", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                yield return new WaitForSeconds(1.5f);
                EventManagement.FinishAscension(false);
            }
        }
    }
}