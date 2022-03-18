using System;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using InscryptionAPI.Card;

namespace Infiniscryption.PackManagement.UserInterface
{
    /// <summary>
    /// A point-in-time cache of all known packs and their contents
    /// 
    /// One of the neat things about the API is that the card list can change dynamically
    /// People can add hooks to the card list management event and add/remove/modify cards as they wish
    /// Which creates a ton of flexibility.
    /// But that means that whenever you look at the card list, it could be different later.
    /// And you don't really want to repeatedly iterate through the card list
    /// 
    /// This class travels through the card list and assigns cards to packs based on on the pack definition
    /// and whether or not the card, by default, has the metacategories that make it available in the first place
    /// </summary>
    public class PackContentCache
    {
        public List<PackInfo> OrderedPacks { get; private set; }

        private Dictionary<string, List<string>> PackMapper;

        public PackContentCache()
        {
            this.OrderedPacks = PackManager.AllPacks.Where(pi => pi.IsBaseGameCardPack)
                                             .Concat(PackManager.AllPacks.Where(pi => pi.IsStandardCardPack))
                                             .Concat(PackManager.AllPacks.Where(pi => pi.IsLeftoversPack))
                                             .Where(pi => pi.ValidFor.Contains(PackInfo.GetTempleDefaultMetacategory(PackManager.ScreenState)))
                                             .ToList();

            // This could be faster but I think this will be good enough
            PackMapper = OrderedPacks.ToDictionary(pi => pi.Key, pi => new List<string>(pi.Cards.Where(ci => ci.CardIsValidForScreenState()).Select(ci => ci.name)));

            foreach (var item in PackMapper)
                PackPlugin.Log.LogInfo($"Pack {item.Key} has {item.Value.Count} cards in it");

            // And now we remove everything that doesn't have cards!
            OrderedPacks.RemoveAll(pi => PackMapper[pi.Key].Count == 0);
        }

        public List<string> GetCardsForPack(PackInfo pack)
        {
            if (!PackMapper.ContainsKey(pack.Key))
                throw new KeyNotFoundException($"I do not recognize pack {pack.Title}");

            return PackMapper[pack.Key];
        }
    }
}