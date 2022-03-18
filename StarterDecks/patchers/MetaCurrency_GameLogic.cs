using InscryptionAPI.Saves;

namespace Infiniscryption.StarterDecks.Patchers
{
    public static partial class MetaCurrencyPatches
    {
        // Here, we establish the metacurrency
        // There are two metacurrencies: excess teeth and quills
        // Excess teeth are teeth that are leftover when you die

        public static int ExcessTeeth
        {
            get { return ModdedSaveManager.SaveData.GetValueAsInt(InfiniscryptionStarterDecksPlugin.PluginGuid, "MetaCurrency.Teeth"); }
            set { ModdedSaveManager.SaveData.SetValue(InfiniscryptionStarterDecksPlugin.PluginGuid, "MetaCurrency.Teeth", value.ToString()); }
        }
    }
}