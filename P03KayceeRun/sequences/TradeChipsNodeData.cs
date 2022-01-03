using HarmonyLib;
using DiskCardGame;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    [HarmonyPatch]
    public class TradeChipsNodeData : SpecialNodeData
    {
        public static HoloMapNode.NodeDataType TradeChipsForCards = (HoloMapNode.NodeDataType)72403;

        [HarmonyPatch(typeof(HoloMapNode), "AssignNodeData")]
        [HarmonyPrefix]
        public static bool PatchTradeSequenceNodeData(ref HoloMapNode __instance)
        {
            if ((int)__instance.NodeType == (int)TradeChipsForCards)
            {
                __instance.Data = new TradeChipsNodeData();
                return false;
            }
            return true;
        }
    }
}