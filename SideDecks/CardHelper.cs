using DiskCardGame;
using Infiniscryption.SideDecks.Patchers;
using InscryptionAPI.Card;

namespace Infiniscryption.SideDecks
{
    public static class CardHelpers
    {
        public static CardInfo SetSideDeck(this CardInfo info, CardTemple temple)
        {
            info.AddMetaCategories(SideDeckManager.SIDE_DECK);
            info.temple = temple;
            return info;
        }
    }
}