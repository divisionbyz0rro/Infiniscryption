using Infiniscryption.Core.Helpers;

namespace Infiniscryption.PackManagement.Patchers
{
    public static class CreatePacks
    {
        public static void CreatePacksForOtherMods()
        {
            // Create a pack for Eri's
            // currently testing jlpk
            PackInfo erisPack = PackManager.GetPackInfo("eri");
            erisPack.Title = "Eri Card Expansion";
            erisPack.SetTexture(AssetHelper.LoadTexture("eris_pack"));
            erisPack.Description = "From the [randomcard] to the [randomcard], this pack contains [count] wild animals that feel right at home in the wild world of Inscryption.";

            PackInfo garethsPack = PackManager.GetPackInfo("Garethmod");
            garethsPack.Title = "Gareth's Mod";
            garethsPack.SetTexture(AssetHelper.LoadTexture("gareths_pack"));
            garethsPack.Description = "With [count] cards and six unique sigils, all which fit right into the look and feel of the main game, [name] was one of the first and most popular expansions for Inscryption. Features cards such as [randomcard], [randomcard], and [randomcard].";

            PackInfo araExpansion = PackManager.GetPackInfo("aracardexpansion");
            araExpansion.Title = "Ara's Card Expansion";
            araExpansion.SetTexture(AssetHelper.LoadTexture("aras_packs"));
            araExpansion.Description = "This expansion contains [count] cards that offer a unique twist on Inscryption's core gameplay. Cards like [randomcard] and [randomcard] will give a little additional spice to your next run.";
        }
    }
}