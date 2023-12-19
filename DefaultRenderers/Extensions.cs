using System;
using DiskCardGame;
using InscryptionAPI.Card;

namespace Infiniscryption.DefaultRenderers
{
    public static class DefaultRenderersExtensions
    {
        public static CardTemple GetRendererTemple(this CardInfo info)
        {
            if (info.HasTrait(Trait.Giant))
                return info.temple;

            string rendererOverrideTemple = info.GetExtendedProperty("Renderer.OverrideTemple");

            if (!string.IsNullOrEmpty(rendererOverrideTemple))
            {
                bool success = Enum.TryParse<CardTemple>(rendererOverrideTemple, out CardTemple rendTemple);
                if (success)
                    return rendTemple;
            }

            if (!DefaultCardRenderer.EnabledForAllCards)
                return DefaultCardRenderer.ActiveTemple.GetValueOrDefault(info.temple);

            string packManagerTemple = info.GetExtendedProperty("PackManager.OriginalTemple");
            if (!string.IsNullOrEmpty(packManagerTemple))
            {
                bool success = Enum.TryParse<CardTemple>(packManagerTemple, out CardTemple packTemple);
                if (success)
                    return packTemple;
            }
            return info.temple;
        }
    }
}