using UnityEngine;
using DiskCardGame;
using Infiniscryption.Core.Helpers;
using Infiniscryption.Spells.Patchers;
using InscryptionAPI.Card;

namespace Infiniscryption.Spells.Sigils
{
    public class GlobalSpellAbility :  VariableStatBehaviour
    {
        // Why is this a stat behavior when these cards have no stats?
        // Simple. I want to cover over the health and attack icons.
        // I want these cards to have 0 health and 0 attack at all times in all zones.
        // This is the best way to do that.

        // I'm following the pattern of HealthForAnts

        private static SpecialStatIcon _icon;
        public static SpecialStatIcon Icon => _icon;
        public override SpecialStatIcon IconType => _icon;

        private static SpecialTriggeredAbility _id;
        public static SpecialTriggeredAbility ID => _id;

        public static void Register()
        {
            StatIconInfo info = ScriptableObject.CreateInstance<StatIconInfo>();
            info.appliesToAttack = true;
            info.appliesToHealth = true;
            info.rulebookName = "Spell (Global)";
            info.rulebookDescription = "When played, this card will cause an immediate effect and then disappear.";
            info.gbcDescription = "Global spell";
            info.iconGraphic = AssetHelper.LoadTexture("global_spell_stat_icon");
            info.SetPixelIcon(AssetHelper.LoadTexture("global_spell_icon_pixel"));
            info.SetDefaultPart1Ability();

            GlobalSpellAbility._icon = StatIconManager.Add(
                InfiniscryptionSpellsPlugin.OriginalPluginGuid,
                info,
                typeof(GlobalSpellAbility)
            ).Id;

            // Honestly, this should be a trait or something.
            // But for backwards compatibility, I'm leaving it.
            GlobalSpellAbility._id = SpecialTriggeredAbilityManager.Add(
                InfiniscryptionSpellsPlugin.OriginalPluginGuid,
                info.rulebookName,
                typeof(GlobalSpellAbility)
            ).Id;

            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(SpellBehavior.SpellBackgroundAppearance).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(SpellBehavior.RareSpellBackgroundAppearance).TypeHandle);
        }

        // No stats for these cards!
        public override int[] GetStatValues()
        {
            return new int[] { 0, 0 };
        }
    }
}