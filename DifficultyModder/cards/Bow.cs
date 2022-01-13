using DiskCardGame;
using HarmonyLib;
using System.Collections.Generic;
using Infiniscryption.Core.Helpers;
using InscryptionAPI.Guid;
using Infiniscryption.Spells.Sigils;
using InscryptionAPI.Card;

namespace Infiniscryption.Curses.Cards
{
    public static class Bow
    {
        public static void RegisterCardAndAbilities(Harmony harmony)
        {
            CardManager.New("Trapper_Bow", "Bow and Arrow", 0, 0)
                .SetTargetedSpell()
                .SetPortrait(AssetHelper.LoadTexture("portrait_bow"))
                .AddAbilities(DirectDamage.AbilityID, DirectDamage.AbilityID);

            CardManager.New("Trapper_Capture", "Capture", 0, 0)
                .SetTargetedSpell()
                .SetPortrait(AssetHelper.LoadTexture("portrait_capture"))
                .AddAbilities(Fishhook.AbilityID);

            CardManager.New("Trapper_Spike_Trap", "Spike Trap", 0, 2)
                .SetPortrait(AssetHelper.LoadTexture("portrait_spike_trap"))
                .SetTerrain()
                .AddAbilities(Ability.Sharp, Ability.DebuffEnemy);
        }
    }
}
