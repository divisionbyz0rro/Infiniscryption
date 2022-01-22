using DiskCardGame;
using Infiniscryption.Spells.Patchers;
using InscryptionAPI.Card;

namespace Infiniscryption.Spells.Sigils
{
    public static class CardHelpers
    {
        public static CardInfo SetGlobalSpell(this CardInfo card)
        {
            card.hideAttackAndHealth = true;
            card.AddSpecialAbilities(GlobalSpellAbility.ID);
            card.specialStatIcon = GlobalSpellAbility.Icon;
            if (card.metaCategories.Contains(CardMetaCategory.Rare))
            {
                card.AddAppearances(SpellBehavior.RareSpellBackgroundAppearance.ID);
                card.appearanceBehaviour.Remove(CardAppearanceBehaviour.Appearance.RareCardBackground);
            }
            else
            {
                card.AddAppearances(SpellBehavior.SpellBackgroundAppearance.ID);
            }
            return card;
        }

        public static CardInfo SetTargetedSpell(this CardInfo card)
        {
            card.hideAttackAndHealth = true;
            card.AddSpecialAbilities(TargetedSpellAbility.ID);
            card.specialStatIcon = TargetedSpellAbility.Icon;
            if (card.metaCategories.Contains(CardMetaCategory.Rare))
            {
                card.AddAppearances(SpellBehavior.RareSpellBackgroundAppearance.ID);
                card.appearanceBehaviour.Remove(CardAppearanceBehaviour.Appearance.RareCardBackground);
            }
            else
            {
                card.AddAppearances(SpellBehavior.SpellBackgroundAppearance.ID);
            }
            return card;
        }
    }
}