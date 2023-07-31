using DiskCardGame;
using HarmonyLib;
using System.Collections.Generic;
using InscryptionAPI.Helpers;
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
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_bow.png", typeof(Bow).Assembly))
                .AddAbilities(DirectDamage.AbilityID, DirectDamage.AbilityID);

            CardManager.New(CursePlugin.CardPrefix, TrapperTraderBossHardOpponent.CAPTURE_CARD, "Capture", 0, 0)
                .SetTargetedSpell()
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_capture.png", typeof(Bow).Assembly))
                .AddAbilities(Fishhook.AbilityID);

            CardManager.New(CursePlugin.CardPrefix, TrapperTraderBossHardOpponent.SPIKE_TRAP_CARD, "Spike Trap", 0, 2)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_spike_trap.png", typeof(Bow).Assembly))
                .AddTraits(Trait.Terrain)
                .AddAppearances(CardAppearanceBehaviour.Appearance.TerrainBackground, CardAppearanceBehaviour.Appearance.TerrainLayout)
                .AddAbilities(Ability.Sharp, Ability.DebuffEnemy);
        }
    }
}
