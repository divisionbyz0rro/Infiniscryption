using DiskCardGame;

namespace Infiniscryption.Spells.Sigils
{
    public static class CardHelpers
    {
        public static CardInfo SetGlobalSpell(this CardInfo card)
        {
            card.hideAttackAndHealth = true;
            card.specialStatIcon = GlobalSpellAbility.Icon;
            card.specialAbilities = new() { GlobalSpellAbility.ID };
            return card;
        }

        public static CardInfo SetTargetedSpell(this CardInfo card)
        {
            card.hideAttackAndHealth = true;
            card.specialStatIcon = TargetedSpellAbility.Icon;
            card.specialAbilities = new() { TargetedSpellAbility.ID };
            return card;
        }
    }
}