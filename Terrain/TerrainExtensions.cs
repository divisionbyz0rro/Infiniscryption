using System;
using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using Pixelplacement;
using UnityEngine;
using HarmonyLib;
using System.Linq;
using InscryptionAPI.Encounters;
using InscryptionAPI.Helpers;
using InscryptionAPI.Ascension;
using InscryptionAPI.Nodes;
using InscryptionAPI.Guid;

namespace Infiniscryption.Terrain
{
    [HarmonyPatch]
    public static class TerrainExtensions
    {
        /// <summary>
        /// Indicates if the card will block attacks
        /// </summary>
        public static bool IsBlocking(this PlayableCard card, bool blockingOpponent) => card.Health > 0 && (!card.HasAbility(Passthrough.AbilityID) || blockingOpponent == card.OpponentCard);

        /// <summary>
        /// Indicates if the slot is blocked by terrain
        /// </summary>
        public static bool SlotIsBlocked(this CardSlot slot, bool blockingOpponent) => slot.Card != null && slot.Card.IsBlocking(blockingOpponent);
    }
}