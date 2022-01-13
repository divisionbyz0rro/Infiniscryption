using DiskCardGame;
using HarmonyLib;
using System.Collections.Generic;
using InscryptionAPI.Card;
using Infiniscryption.Spells.Sigils;
using UnityEngine;
using Infiniscryption.Core.Helpers;

namespace Infiniscryption.Spells.Patchers
{
    public static class SpellCards
    {
        internal static void RegisterCustomCards(Harmony harmony)
        {

            CardManager.New(
                    "Spell_Kettle_of_Avarice", 
                    "Kettle of Avarice", 
                    0, 0, // attack/health
                    "It allows you to draw two more cards")
                .SetDefaultPart1Card()
                .SetPortrait(AssetHelper.LoadTexture("kettle_of_avarice"))
                .SetGlobalSpell()
                .SetCost(bloodCost: 1)
                .AddAbilities(DrawTwoCards.AbilityID);

            CardManager.New(
                    "Spell_Anger_of_the_Gods", 
                    "Anger of the Gods", 
                    0, 0, // attack/health
                    "For when nothing else will do the trick")
                .SetDefaultPart1Card()
                .SetPortrait(AssetHelper.LoadTexture("anger_of_all"))
                .SetGlobalSpell()
                .SetRare()
                .SetCost(bloodCost: 2)
                .AddAbilities(DestroyAllCardsOnDeath.AbilityID);

            CardManager.New(
                    "Spell_Lightning", 
                    "Lightning", 
                    0, 0, // attack/health
                    "A perfectly serviceable amount of damage")
                .SetDefaultPart1Card()
                .SetPortrait(AssetHelper.LoadTexture("lightning_bolt"))
                .SetTargetedSpell()
                .SetCost(bloodCost: 1)
                .AddAbilities(DirectDamage.AbilityID, DirectDamage.AbilityID);

            CardManager.New(
                    "Spell_Backpack", 
                    "Trip to the Store", 
                    0, 0, // attack/health
                    "Send one of your creatures on a trip to the store. Who knows what they will come back with")
                .SetDefaultPart1Card()
                .SetPortrait(AssetHelper.LoadTexture("backpack"))
                .SetGlobalSpell()
                .SetCost(bloodCost: 2)
                .AddAbilities(Ability.RandomConsumable);

            CardManager.New(
                    "Spell_Rot_Healing", 
                    "Rot Healing", 
                    0, 0, // attack/health
                    "Restores just a little bit of health")
                .SetDefaultPart1Card()
                .SetPortrait(AssetHelper.LoadTexture("plague_doctor"))
                .SetTargetedSpell()
                .SetCost(bonesCost: 1)
                .AddAbilities(DirectHeal.AbilityID, DirectHeal.AbilityID);

            CardManager.New(
                    "Spell_Dammed_up", 
                    "Dammed Up", 
                    0, 0, // attack/health
                    "So many dams...")
                .SetDefaultPart1Card()
                .SetPortrait(AssetHelper.LoadTexture("dammed_up"))
                .SetTargetedSpell()
                .SetCost(bloodCost: 1)
                .AddAbilities(Ability.AllStrike, Ability.CreateDams);

            CardManager.New(
                    "Spell_Irritate", 
                    "Irritate", 
                    0, 0, // attack/health
                    "This is what happens when you poke the bear...or wolf")
                .SetDefaultPart1Card()
                .SetPortrait(AssetHelper.LoadTexture("snarling_wolf"))
                .SetTargetedSpell()
                .SetCost(bonesCost: 2)
                .AddAbilities(AttackBuff.AbilityID, DirectDamage.AbilityID);

            CardManager.New(
                    "Spell_Compost", 
                    "Compost", 
                    0, 0, // attack/health
                    "Time to recycle those old bones")
                .SetDefaultPart1Card()
                .SetPortrait(AssetHelper.LoadTexture("compost"))
                .SetGlobalSpell()
                .SetCost(bonesCost: 5)
                .AddAbilities(DrawTwoCards.AbilityID);

            CardManager.New(
                    "Spell_Fetch", 
                    "Go Fetch", 
                    0, 0, // attack/health
                    "Good doggy")
                .SetDefaultPart1Card()
                .SetPortrait(AssetHelper.LoadTexture("wolf_fetch"))
                .SetGlobalSpell()
                .AddAbilities(Ability.QuadrupleBones);
        }
    }
}