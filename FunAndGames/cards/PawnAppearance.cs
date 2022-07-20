using InscryptionAPI.Card;
using DiskCardGame;
using Infiniscryption.Core.Helpers;
using UnityEngine;
using System.Collections.Generic;
using InscryptionAPI.Helpers;

namespace Infiniscryption.FunAndGames.Cards
{
    public class PawnAppearance : CardAppearanceBehaviour
    {
        public static CardAppearanceBehaviour.Appearance ID { get; private set; }

        private static Sprite LEFT_PORTRAIT = TextureHelper.ConvertTexture(AssetHelper.LoadTexture("pawn_portrait_left"), TextureHelper.SpriteType.CardPortrait);
        private static Sprite RIGHT_PORTRAIT = TextureHelper.ConvertTexture(AssetHelper.LoadTexture("pawn_portrait_right"), TextureHelper.SpriteType.CardPortrait);
        private static Sprite MIDDLE_PORTRAIT = TextureHelper.ConvertTexture(AssetHelper.LoadTexture("pawn_portrait_middle"), TextureHelper.SpriteType.CardPortrait);
        private static Sprite NONE_PORTRAIT = TextureHelper.ConvertTexture(AssetHelper.LoadTexture("pawn_portrait_none"), TextureHelper.SpriteType.CardPortrait);

        public override void ApplyAppearance()
        {
            GamesPlugin.Log.LogDebug($"Applying pawn appearance");

            if (this.Card is PlayableCard pCard)
            {
                if (pCard.slot == null)
                    return;

                PawnStrike pawnStrikeBehavior = this.Card.GetComponent<PawnStrike>();
                if (pawnStrikeBehavior == null)
                    return;

                List<CardSlot> slots = pawnStrikeBehavior.GetOpposingSlots(null, null);

                GamesPlugin.Log.LogDebug($"Applying targeted pawn appearance");

                if (slots == null)
                    pCard.renderInfo.portraitOverride = MIDDLE_PORTRAIT;
                else if (slots.Count == 0)
                    pCard.renderInfo.portraitOverride = NONE_PORTRAIT;
                else if (slots[0] == pCard.Slot.opposingSlot)
                    pCard.renderInfo.portraitOverride = MIDDLE_PORTRAIT;
                else if (slots[0].Index < pCard.slot.Index)
                    pCard.renderInfo.portraitOverride = LEFT_PORTRAIT;
                else
                    pCard.renderInfo.portraitOverride = RIGHT_PORTRAIT;
            }
        }

        public override void OnPreRenderCard()
        {
            this.ApplyAppearance();
        }

        internal static void Register()
        {
            ID = CardAppearanceBehaviourManager.Add(GamesPlugin.PluginGuid, "PawnAppearance", typeof(PawnAppearance)).Id;
        }
    }
}