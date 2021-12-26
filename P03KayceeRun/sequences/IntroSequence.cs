using System.Collections;
using DiskCardGame;
using HarmonyLib;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    [HarmonyPatch]
    public static class IntroSequence
    {
        [HarmonyPatch(typeof(Part3GameFlowManager), "SceneSpecificInitialization")]
        [HarmonyPrefix]
        public static bool ForceAscensionToStart()
        {
            if (SaveFile.IsAscension)
            {
                ItemsManager.Instance.SetSlotsAtEdge(true, true);
                Part3GameFlowManager.Instance.StartCoroutine(ReplaceIntroSequenceForAscension());
                return false;
            }
            return true;
        }

        public static IEnumerator ReplaceIntroSequenceForAscension()
        {
            PauseMenu.pausingDisabled = false;
            HoloGameMap.Instance.HideMapImmediate();
            UIManager.Instance.Effects.GetEffect<ScreenColorEffect>().SetColor(GameColors.Instance.nearWhite);
            UIManager.Instance.Effects.GetEffect<ScreenColorEffect>().SetAlpha(1f);
            UIManager.Instance.Effects.GetEffect<ScreenColorEffect>().SetIntensity(0f, 0.4f);
            ViewManager.Instance.SwitchToView(View.MapDeckReview, true, false);
            yield return new WaitForSeconds(1f);
            //DeckReviewSequencer.Instance.SetDeckReviewShown(true, null, HoloGameMap.Instance.DefaultPosition, true, false);
            ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;
            yield return new WaitUntil(() => ViewManager.Instance.CurrentView != View.MapDeckReview);
            ViewManager.Instance.SwitchToView(View.MapDefault, false, true);
            Part3GameFlowManager.Instance.StartGameStateDirect(GameState.Map);
            yield return new WaitUntil(() => !HoloGameMap.Instance.FullyUnrolled);
	        yield return new WaitUntil(() => HoloGameMap.Instance.FullyUnrolled);
        }
    }
}