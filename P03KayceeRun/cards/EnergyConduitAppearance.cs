using InscryptionAPI.Card;
using DiskCardGame;
using Infiniscryption.Core.Helpers;
using UnityEngine;
using System.Collections.Generic;
using InscryptionAPI.Helpers;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class EnergyConduitAppearnace : CardAppearanceBehaviour
    {
        public static CardAppearanceBehaviour.Appearance ID { get; private set; }

        private static Sprite[] PORTRAITS = new Sprite[] {
            TextureHelper.ConvertTexture(AssetHelper.LoadTexture("portrait_conduitenergy"), TextureHelper.SpriteType.CardPortrait),
            TextureHelper.ConvertTexture(AssetHelper.LoadTexture("portrait_conduitenergy_1"), TextureHelper.SpriteType.CardPortrait),
            TextureHelper.ConvertTexture(AssetHelper.LoadTexture("portrait_conduitenergy_2"), TextureHelper.SpriteType.CardPortrait),
            TextureHelper.ConvertTexture(AssetHelper.LoadTexture("portrait_conduitenergy_3"), TextureHelper.SpriteType.CardPortrait)
        };

        public override void ApplyAppearance()
        {
            if (this.Card is PlayableCard pCard)
            {
                if (pCard.slot == null)
                    return;

                NewConduitEnergy behaviour = this.Card.GetComponent<NewConduitEnergy>();
                if (behaviour == null)
                    return;

                if (!behaviour.CompletesCircuit())
                    pCard.renderInfo.portraitOverride = PORTRAITS[0];
                else
                    pCard.renderInfo.portraitOverride = PORTRAITS[behaviour.RemainingEnergy];
            }
        }

        public override void OnPreRenderCard()
        {
            this.ApplyAppearance();
        }

        internal static void Register()
        {
            ID = CardAppearanceBehaviourManager.Add(P03Plugin.PluginGuid, "EnergyConduitAppearance", typeof(EnergyConduitAppearnace)).Id;
        }
    }
}