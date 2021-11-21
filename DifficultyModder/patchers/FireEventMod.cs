using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using DiskCardGame;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine.UI;

namespace Infiniscryption.DifficultyMod.Patchers
{
    public static class FireEventMod
    {
        // The goal of this mod is to make the fire event have a little more
        // at stake. Right now, you either lose a bad card or buff a bad card.
        // Not much is at stake.

        // 1) Make the odds of failure higher
        // 2) Make the cost of failure higher
        //    - If they eat your card, they also come for you
        //    - They take a consumable - the most recent one you got
        //    - Obviously you don't get the piggy bank
        //    - If you don't have a consumable, they
        //      take the best card in your deck


    }
}