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

        public string ModPrefix;

        public string PackArt;

        public string[] ValidFor;

        public void Convert()
        {
            PackInfo info = PackManager.GetPackInfo(this.ModPrefix);
            info.Title = Title;
            info.Description = Description;
            info.SetTexture(TextureHelper.GetImageAsTexture(PackArt));
            
            if (this.ValidFor != null && this.ValidFor.Length > 0)
            {
                info.ValidFor.Clear();
                info.ValidFor.AddRange(this.ValidFor.Select(s => (PackInfo.PackMetacategory)Enum.Parse(typeof(PackInfo.PackMetacategory), s)));
            }
        }
    }
}