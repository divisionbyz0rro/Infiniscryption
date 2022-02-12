using Infiniscryption.Core.Helpers;

namespace Infiniscryption.PackManagement.Patchers
{
    public static class CreatePacks
    {
        public static void CreatePacksForOtherMods()
        {
            // Create a pack for Eri's
            PackInfo erisPack = new();
            erisPack.Name = "eris_pack";
            erisPack.Title = "Eri Card Expansion";
            erisPack.SetTexture(AssetHelper.LoadTexture("eris_pack"));
            erisPack.Description = "From the [randomcard] to the [randomcard], this pack contains [count] wild animals that feel right at home in the wild world of Inscryption.";
            erisPack.RegexMatch = "^Eri_.*";

            PackInfo garethsPack = new();
            garethsPack.Name = "gareths_mod";
            garethsPack.Title = "Gareth's Mod";
            garethsPack.SetTexture(AssetHelper.LoadTexture("gareths_pack"));
            garethsPack.Description = "With [count] cards and six unique sigils, all which fit right into the look and feel of the main game, [name] was one of the first and most popular expansions for Inscryption. Features cards such as [randomcard], [randomcard], and [randomcard].";
            garethsPack.RegexMatch = "^Gareth.*";

            //PackManager.Add("eri.inscryption.eriscardexpansion", erisPack);
            PackManager.Add("gareth48.inscryption.garethmod", garethsPack);

            PackInfo araExpansion = new();
            araExpansion.Name = "ara_expansion";
            araExpansion.Title = "Ara's Card Expansion";
            araExpansion.SetTexture(AssetHelper.LoadTexture("aras_packs"));
            araExpansion.Description = "This expansion contains [count] cards that offer a unique twist on Inscryption's core gameplay. Cards like [randomcard] and [randomcard] will give a little additional spice to your next run.";
            araExpansion.Cards = new System.Collections.Generic.List<string>() {
                "cobra",
                "inkygolem",
                "inkygolemoffense",
                "totem_bird",
                "totem_hoove",
                "totem_squirrel",
                "totem_lizard",
                "totem_bug",
                "totem_wolf",
                "anthill",
                "augmented_geck",
                "beast",
                "beast_2",
                "beast_3",
                "big_ant",
                "bird_god",
                "Blobfish",
                "notbug",
                "Bullet_ant",
                "bush_elk",
                "caninegod",
                "Snake_corn",
                "crow",
                "corpsewalker",
                "elk_old",
                "Fly_trap",
                "hoovedgod",
                "insect_god",
                "Elk_leader",
                "Mites",
                "smallmoon",
                "owl",
                "snake_god",
                "riverbird",
                "riverbird_water",
                "Screeching_ant",
                "small_ant",
                "Soldier_ant",
                "Spider",
                "Squid",
                "squirrelgod",
                "sun_priestess",
                "hole",
                "ThornyDevil",
                "twintailed_lizard",
                "reanimator",
                "undeaddog",
                "Vicious_Beaver",
                "warriorsquirrel",
                "wasp",
                "fish",
                "dam_sharp",
                "unknownarmy",
                "Ex_Queenbee",
                "bee_drone",
                "krampus",
                "present",
                "snowball",
                "snowman"
            };

            PackManager.Add("arackulele.inscryption.aracardexpansion", araExpansion);
        }
    }
}