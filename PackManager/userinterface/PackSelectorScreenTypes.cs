using InscryptionAPI.Ascension;

namespace Infiniscryption.PackManagement.UserInterface
{
    [AscensionScreenSort(AscensionScreenSort.Direction.RequiresStart)]
    public class CardPackScreen : PackSelectorScreenBase<PackInfo>
    {
        public override string headerText => "Select Card Packs";
    }

    [AscensionScreenSort(AscensionScreenSort.Direction.PrefersStart)]
    public class EncounterPackScreen : PackSelectorScreenBase<EncounterPackInfo>
    {
        public override string headerText => "Select Encounter Packs";
    }

}