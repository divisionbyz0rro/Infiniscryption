using DiskCardGame;
using Infiniscryption.Core.Helpers;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Card;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public class ForceRevolverAppearance : CardAppearanceBehaviour
    {
        public static CardAppearanceBehaviour.Appearance ID { get; private set; }

        public override void ApplyAppearance()
        {
			if (base.Card.Anim is DiskCardAnimationController dac)
				dac.SetWeaponMesh(DiskCardWeapon.Revolver);
        }

        public static void Register()
        {
            ID = CardAppearanceBehaviourManager.Add(P03Plugin.PluginGuid, "ForceRevolverAppearance", typeof(ForceRevolverAppearance)).Id;
        }
    }
}