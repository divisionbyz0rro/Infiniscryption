using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using DiskCardGame;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System;
using Infiniscryption.Core.Helpers;
using Infiniscryption.Spells.Sigils;
using Infiniscryption.Spells.Patchers;
using InscryptionAPI.Card;

namespace Infiniscryption.Spells
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("cyantist.inscryption.api")]
    public class InfiniscryptionSpellsPlugin : BaseUnityPlugin
    {

        internal const string PluginGuid = "zorro.inscryption.infiniscryption.spells";
		internal const string PluginName = "Infiniscryption Spells";
		internal const string PluginVersion = "1.0";

        internal static ManualLogSource Log;

        private bool AddCards
        {
            get
            {
                return Config.Bind("InfiniscryptionSpells", "AddCards", false, new BepInEx.Configuration.ConfigDescription("If true, this will add the sample cards to the card pool.")).Value;
            }
        }

        private void Awake()
        {
            Log = base.Logger;

            Harmony harmony = new Harmony(PluginGuid);

            harmony.PatchAll(typeof(SpellBehavior));

            TargetedSpellAbility.Register();
            GlobalSpellAbility.Register();
            DrawTwoCards.Register();
            DestroyAllCardsOnDeath.Register();
            DirectDamage.Register();
            DirectHeal.Register();
            AttackBuff.Register();
            AttackNerf.Register();
            Fishhook.Register();
            

            if (AddCards)
            {
                SpellCards.RegisterCustomCards(harmony);
            }

            // This makes sure that all cards with the spell ability are properly given all of the various
            // components of a spell
            CardManager.ModifyCardList += delegate(List<CardInfo> cards)
            {
                foreach (CardInfo card in cards)
                {
                    if (card.IsTargetedSpell())
                        card.SetTargetedSpell();

                    if (card.IsGlobalSpell())
                        card.SetGlobalSpell();
                }
                return cards;
            };

            Logger.LogInfo($"Plugin {PluginName} is loaded!");
        }
    }
}
