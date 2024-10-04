using System;
using System.Linq;
using DiskCardGame;
using InscryptionAPI.Helpers;

namespace Infiniscryption.PackManagement.Patchers
{
    public class PackInfoJSON
    {
        internal Type PackType = typeof(PackInfo);

        public string Title;

        public string Description;

        public string ModPrefix;

        public string PackArt;

        public string[] ValidFor;

        // public string[] AdditionalEncounters;

        // public string[] AdditionalRegions;

        public bool SplitPackByCardTemple;

        public void Convert()
        {
            PackInfoBase info = PackManager.GetPackInfo(PackType, this.ModPrefix);
            info.Title = Title;
            info.Description = Description;
            info.SetTexture(TextureHelper.GetImageAsTexture(PackArt));
            info.SplitPackByCardTemple = SplitPackByCardTemple;

            if (this.ValidFor != null && this.ValidFor.Length > 0)
            {
                info.ValidFor.Clear();
                info.ValidFor.AddRange(this.ValidFor.Select(s => (PackInfo.PackMetacategory)Enum.Parse(typeof(PackInfo.PackMetacategory), s)));
            }

            // if (PackType == typeof(EncounterPackInfo))
            // {
            //     if (this.AdditionalEncounters != null && this.AdditionalEncounters.Length > 0)
            //     {
            //         (info as EncounterPackInfo).AdditionalEncounters = new(this.AdditionalEncounters);
            //     }
            //     if (this.AdditionalRegions != null && this.AdditionalRegions.Length > 0)
            //     {
            //         (info as EncounterPackInfo).AdditionalRegions = new(this.AdditionalRegions);
            //     }
            // }
        }
    }
}