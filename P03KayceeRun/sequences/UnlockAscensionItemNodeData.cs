using HarmonyLib;
using DiskCardGame;
using InscryptionAPI.Guid;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    [HarmonyPatch]
    public class UnlockAscensionItemNodeData : SpecialNodeData
    {
        public static readonly HoloMapNode.NodeDataType UnlockItemsAscension = GuidManager.GetEnumValue<HoloMapNode.NodeDataType>(P03Plugin.PluginGuid, "UnlockAscensionItemNodeData");

        [HarmonyPatch(typeof(HoloMapNode), "AssignNodeData")]
        [HarmonyPrefix]
        public static bool PatchTradeSequenceNodeData(ref HoloMapNode __instance)
        {
            if (__instance.NodeType == UnlockItemsAscension)
            {
                __instance.Data = new UnlockAscensionItemNodeData();
                return false;
            }
            return true;
        }
    }
}