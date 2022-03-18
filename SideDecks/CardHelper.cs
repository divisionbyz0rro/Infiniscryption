using DiskCardGame;
using Infiniscryption.SideDecks.Patchers;
using InscryptionAPI.Card;

namespace Infiniscryption.SideDecks
{
    public static class CardHelpers
    {
        public static CardInfo SetSideDeck(this CardInfo info, CardTemple temple, int sideDeckValue)
        {
            info.AddMetaCategories(SideDeckManager.SIDE_DECK);
            info.temple = temple;
            info.SetExtendedProperty("SideDeckValue", sideDeckValue);
            return info;
        }

        public static int GetSideDeckValue(this CardInfo info)
        {
            int? value = info.GetExtendedPropertyAsInt("SideDeckValue");
            return value.HasValue ? value.Value : 0;
        }
    }
}