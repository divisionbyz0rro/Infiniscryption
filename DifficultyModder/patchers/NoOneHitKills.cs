using DiskCardGame;
using HarmonyLib;
using Infiniscryption.Core.Helpers;
using InscryptionAPI.Ascension;

namespace Infiniscryption.Curses.Patchers
{
    public static class NoOneHitKills
    {
        public static AscensionChallenge ID {get; private set;}

        public static void Register(Harmony harmony)
        {
            ID = ChallengeManager.Add
            (
                CursePlugin.PluginGuid,
                "Two Turn Minimum",
                "You cannot deal lethal damage on the first turn.",
                10,
                AssetHelper.LoadTexture("challenge_no_ohk")
            ).challengeType;

            harmony.PatchAll(typeof(NoOneHitKills));
        }

        [HarmonyPatch(typeof(LifeManager), nameof(LifeManager.ShowDamageSequence))]
        [HarmonyPrefix]
        public static void StopLethalTurnOneDamage(ref int damage, ref int numWeights, bool toPlayer)
        {
            if (!AscensionSaveData.Data.ChallengeIsActive(ID) || toPlayer || TurnManager.Instance.TurnNumber > 1 || damage < LifeManager.Instance.DamageUntilPlayerWin)
                return;

            ChallengeActivationUI.Instance.ShowActivation(ID);

            damage = LifeManager.Instance.DamageUntilPlayerWin - 1;
            numWeights = damage;
        }
    }
}