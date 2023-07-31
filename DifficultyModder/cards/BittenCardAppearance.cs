using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using DiskCardGame;
using System.Linq;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;

namespace Infiniscryption.Curses.Cards
{
    public class BittenCardAppearance : CardAppearanceBehaviour
    {
        public static CardAppearanceBehaviour.Appearance ID { get; private set;}

        private static Texture2D _sharkBiteDecal = TextureHelper.GetImageAsTexture("shark_bite_decal.png", typeof(BittenCardAppearance).Assembly);
        private static Texture2D _sharkBiteBackground = TextureHelper.GetImageAsTexture("card_empty_sharkbite.png", typeof(BittenCardAppearance).Assembly);

        public override void ApplyAppearance()
        {
            Card.Info.temporaryDecals = new();

            Card.RenderInfo.baseTextureOverride = _sharkBiteBackground;

            if (Card.Info.TempDecals.Any(t => t.name == _sharkBiteDecal.name))
                return;

            Card.Info.TempDecals.Add(_sharkBiteDecal);
        }

        public static void Register()
        {
            ID = CardAppearanceBehaviourManager.Add(CursePlugin.PluginGuid, "BittenByShark", typeof(BittenCardAppearance)).Id;
        }
    }
}