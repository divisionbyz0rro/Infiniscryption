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
using Infiniscryption.Curses.Helpers;
using Infiniscryption.Curses.Sequences;
using Infiniscryption.Core.Helpers;

namespace Infiniscryption.Curses.Patchers
{
    public class LooseTeeth : CurseBase
    {
        public LooseTeeth(string id, GetActiveDelegate getActive, SetActiveDelegate setActive) : base(id, getActive, setActive) { }

        public override string Description => "You will start every game with a tooth on your side of the scale";

        public override string Title => "Loose Teeth";

        Texture2D _iconTexture = AssetHelper.LoadTexture("tooth_icon");
        public override Texture2D IconTexture => _iconTexture;

        public override void Reset()
        {
            // Nothing needs to happen on a run reset
        }

        [HarmonyPatch(typeof(TurnManager), "SetupPhase")]
        [HarmonyPostfix]
        public static IEnumerator AddToothAtStartOfGame(IEnumerator sequenceResult)
        {
            while (sequenceResult.MoveNext())
                yield return sequenceResult.Current;

            if (CurseManager.IsActive<LooseTeeth>())
            {
                // Now we add a tooth to the scale
                yield return LifeManager.Instance.ShowDamageSequence(1, 1, true, 0f, null, 0f);
                yield return new WaitForSeconds(0.5f);
                ViewManager.Instance.SwitchToView(View.Default);
            }

            yield break;
        }
    }
}