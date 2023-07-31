using InscryptionAPI.Helpers;

namespace Infiniscryption.PackManagement.Patchers
{
    public static class CreatePacks
    {
        public static void CreatePacksForOtherMods()
        {
            PackInfo erisPack = PackManager.GetPackInfo("eri");
            erisPack.Title = "Eri Card Expansion";
            erisPack.SetTexture(TextureHelper.GetImageAsTexture("eris_pack.png", typeof(CreatePacks).Assembly));
            erisPack.Description = "From the [randomcard] to the [randomcard], this pack contains [count] wild animals that feel right at home in the wild world of Inscryption.";

            // PackInfo garethsPack = PackManager.GetPackInfo("Garethmod");
            // garethsPack.Title = "Gareth's Mod";
            // garethsPack.SetTexture(TextureHelper.GetImageAsTexture("gareths_pack"));
            // garethsPack.Description = "With [count] cards and six unique sigils, all which fit right into the look and feel of the main game, [name] was one of the first and most popular expansions for Inscryption. Features cards such as [randomcard], [randomcard], and [randomcard].";

            PackInfo araExpansion = PackManager.GetPackInfo("aracardexpansion");
            araExpansion.Title = "Ara's Card Expansion";
            araExpansion.SetTexture(TextureHelper.GetImageAsTexture("aras_packs.png", typeof(CreatePacks).Assembly));
            araExpansion.Description = "This expansion contains [count] cards that offer a unique twist on Inscryption's core gameplay. Cards like [randomcard] and [randomcard] will give a little additional spice to your next run.";

            PackInfo hePack = PackManager.GetPackInfo("HE");
            hePack.Title = "Hallownest Expansion";
            hePack.SetTexture(TextureHelper.GetImageAsTexture("he_pack.png", typeof(CreatePacks).Assembly));
            hePack.Description = "A large expansion containing [count] creatures from Hollow Knight. Up from peaceful Crossroads, down into the Abyss.";
        }
    }
}