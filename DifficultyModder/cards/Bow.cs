using DiskCardGame;
using HarmonyLib;
using System.Collections.Generic;
using Infiniscryption.Core.Helpers;
using InscryptionAPI.Guid;
using Infiniscryption.Spells.Sigils;
using InscryptionAPI.Card;
using Infiniscryption.Curses.Sequences;

namespace Infiniscryption.Curses.Cards
{
    public static class Bow
    {
        public static void RegisterCardAndAbilities(Harmony harmony)
        {
            CardManager.New(CursePlugin.CardPrefix, TrapperTraderBossHardOpponent.BOW_CARD, "Bow and Arrow", 0, 0)
                .SetTargetedSpell()
                .SetPortrait(AssetHelper.LoadTexture("portrait_bow"))
                .AddAbilities(DirectDamage.AbilityID, DirectDamage.AbilityID);

            CardManager.New(CursePlugin.CardPrefix, TrapperTraderBossHardOpponent.CAPTURE_CARD, "Capture", 0, 0)
                .SetTargetedSpell()
                .SetPortrait(AssetHelper.LoadTexture("portrait_capture"))
                .AddAbilities(Fishhook.AbilityID);

            CardManager.New(CursePlugin.CardPrefix, TrapperTraderBossHardOpponent.SPIKE_TRAP_CARD, "Spike Trap", 0, 2)
                .SetPortrait(AssetHelper.LoadTexture("portrait_spike_trap"))
                .SetTerrain()
                .AddAbilities(Ability.Sharp, Ability.DebuffEnemy);
        }
    }
}
