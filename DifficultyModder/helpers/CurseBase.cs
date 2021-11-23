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
using Infiniscryption.Core.Helpers;

namespace Infiniscryption.Curses.Helpers
{
    public abstract class CurseBase
    {
        public CurseBase(string id, GetActiveDelegate getActive, SetActiveDelegate setActive)
        {
            this.ID = id;
            this.GetActive = getActive;
            this.SetActive = setActive;
        }


        // Don't mess with these. They should only be set by the manager
        public delegate bool GetActiveDelegate();
        public delegate void SetActiveDelegate(bool active);
        private GetActiveDelegate GetActive;
        private SetActiveDelegate SetActive;

        public string ID { get; private set; }

        // This tells whether or not the difficult mod is active
        public bool Active
        {
            get { return GetActive(); }
            set { SetActive(value); }
        }

        // Describes what the mod does
        public abstract string Description { get; }

        public abstract string Title { get; }

        public abstract Texture2D IconTexture { get; }

        // This is executed whenever a run resets.
        public abstract void Reset();

        private static Texture2D _boonBG;
        public Texture2D CurseBackground
        {
            get
            {
                if (_boonBG == null)
                    _boonBG = AssetHelper.LoadTexture("boon_back_empty");

                return _boonBG;
            }
        }

        private static Texture2D _curseBack;
        public Texture2D CurseCardBack
        {
            get
            {
                if (_curseBack == null)
                    _curseBack = AssetHelper.LoadTexture("boon_flipped");

                return _curseBack;
            }
        }
    }
}