using System.Collections;
using DiskCardGame;
using System.Linq;
using Infiniscryption.P03KayceeRun.Patchers;
using UnityEngine;
using System.Collections.Generic;
using Infiniscryption.Core.Helpers;
using HarmonyLib;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    [HarmonyPatch]
    public class PhotographerAscensionSequencer : PhotographerBattleSequencer
    {
        public override EncounterData BuildCustomEncounter(CardBattleNodeData nodeData)
        {
            EncounterData encounterData = base.BuildCustomEncounter(nodeData);
            EncounterBlueprintData blueprint = (new EncounterBlueprintHelper(AssetHelper.GetResourceString("PhotographerBossP1", "dat"))).AsBlueprint();
            encounterData.opponentTurnPlan = EncounterBuilder.BuildOpponentTurnPlan(blueprint, EventManagement.EncounterDifficulty, false);
            return encounterData;
        }

        
        [HarmonyPatch(typeof(Opponent), nameof(Opponent.ReplaceBlueprint))]
        [HarmonyPostfix]
        public static IEnumerator Postfix(IEnumerator sequence, string blueprintId, bool removeLockedCards = false)
        {
            if (!SaveFile.IsAscension || !(TurnManager.Instance.opponent is PhotographerBossOpponent) || !blueprintId.Equals("PhotographerBossP2"))
            {
                yield return sequence;
                yield break;
            }

            TurnManager.Instance.Opponent.Blueprint = (new EncounterBlueprintHelper(AssetHelper.GetResourceString(blueprintId, "dat"))).AsBlueprint();
            
            List<List<CardInfo>> plan = EncounterBuilder.BuildOpponentTurnPlan(TurnManager.Instance.Opponent.Blueprint, EventManagement.EncounterDifficulty, removeLockedCards);
            TurnManager.Instance.Opponent.ReplaceAndAppendTurnPlan(plan);
            yield return TurnManager.Instance.Opponent.QueueNewCards(true, true);
            yield break;
        }
    }
}