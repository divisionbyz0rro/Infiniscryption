using System;
using System.Linq;
using DiskCardGame;
using InscryptionAPI.Helpers;

namespace Infiniscryption.PackManagement.Patchers
{
    public class PackInfoJSON
    {
        public string Title;

        public string Description;

        public string[] Cards;

        public string RegexMatch;

        public string PackArt;

        public string[] ValidFor;

        public PackInfo Convert()
        {
            PackInfo info = new PackInfo();
            info.Title = Title;
            info.Description = Description;
            info.Cards = Cards == null ? new() : new(Cards);
            info.RegexMatch = RegexMatch;
            info.SetTexture(TextureHelper.GetImageAsTexture(PackArt));
            info.ValidFor = ValidFor == null ? new () : ValidFor.Select(s => (Opponent.Type)Enum.Parse(typeof(Opponent.Type), s)).ToList();
            if (info.ValidFor.Count == 0)
                info.ValidFor.Add(Opponent.Type.Default);
            return info;
        }
    }
}