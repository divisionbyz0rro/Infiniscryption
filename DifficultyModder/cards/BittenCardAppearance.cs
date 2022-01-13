using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using DiskCardGame;
using Infiniscryption.Core.Helpers;
using System.Linq;
using InscryptionAPI.Card;

namespace Infiniscryption.Curses.Cards
{
    public class BittenCardAppearance : CardAppearanceBehaviour
    {
        public static CardAppearanceBehaviour.Appearance ID { get; private set;}

        private static Texture2D _sharkBiteDecal = AssetHelper.LoadTexture("shark_bite_decal");
        private static Texture2D _sharkBiteBackground = AssetHelper.LoadTexture("card_empty_sharkbite");

        public override void ApplyAppearance()
        {
            Card.RenderInfo.baseTextureOverride = _sharkBiteBackground;

            if (Card.Info.TempDecals.Any(t => t.name == _sharkBiteDecal.name))
                return;

            Card.Info.TempDecals.Add(_sharkBiteDecal);
        }

        public static void Register()
        {
            CardAppearanceBehaviourManager.Add(InfiniscryptionCursePlugin.PluginGuid, "BittenByShark", typeof(BittenCardAppearance));
        }
    }
}