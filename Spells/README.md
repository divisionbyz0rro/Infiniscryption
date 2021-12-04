# Spell Cards

This mod contains special abilities that allow you to create a new type of card called a 'spell.' Spells are cards that

a) Do not need a space on board to resolve
b) Die immediately when played.

In other words, these are cards you play entirely because they have an immediate effect.

There are also some additional sigils in this pack that might be useful for you when creating spells.

There are two types of spells:

**Targeted Spells:** These have an effect on one specific space on the board. Use this type if you want to use sigils like 'Direct Damage' (included in this pack) or something like the Beaver's dam creation ability.
**Global Spells:** These have an immediate, global effect when played. If you attach a sigil that expects to be in a specific slot on board, there may be unexpected behavior. For example, the Beaver's dam ability will more than likely give Leshy a free dam.

## Credits

This mod would not be possible without signifcant contributions from @Kopie in the Inscryption Modding discord channel.

## Does this pack add any cards?

It can, but it doesn't by default. If you want my example cards added to the card pool, go to the config file 'zorro.inscryption.infiniscryption.spells.cfg' and set 'AddCards' to true.

This will add the following cards:

- **Kettle of Avarice**: 1 blood, draws two cards.
- **Anger of the Gods**: 1 blood, destroys all creatures on board (Rare).
- **Lightning**: 1 blood, deals 2 damage to a card slot.
- **Trip to the Store**: 1 blood, generates a random consumable.

These cards are not meant to be balanced, but rather to demonstrate how the mod works (hence why they are not added by default).

## Future enhancements:

1. More sigils: More sigils that cover common use cases will be added to the pack to help you develop spell cards.
2. Spell starter pack: A separate mod that adds a starter pack of spells.

## Requirements

As with most mods, you need [BepInEx](https://inscryption.thunderstore.io/package/BepInEx/BepInExPack_Inscryption/) installed. 

You will also need the [API](https://inscryption.thunderstore.io/package/API_dev/API/) installed.

## I want to make a spell - how does this work?

When a spell is played, it will fire either three or four triggers (depending upon the type of spell) in this specific order.

1. PlayFromHand
2. ResolveOnBoard (if this is a targeted spell, the card's slot will be set to the slot that was targeted - otherwise, the card slot will be null)
3. SlotTargetedForAttack (only if this is a targeted spell)
4. Die

As a card developer, it is up to you to put sigils (either existing or custom) on the card to actually make it have an effect when played.

## Adding a spell through the API

To add a spell using the API, you need to add both a 'special ability' and a 'stat icon'. 

The plugin will take care of adding all of the necessary card appearance behaviors for you.

```
using APIPlugin;
using Infiniscryption.Spells.Sigils;

NewCard.Add(
    "Kettle_of_Avarice",
    "Kettle of Avarice",
    0, 0,
    new List<CardMetaCategory>() { CardMetaCategory.ChoiceNode, CardMetaCategory.TraderOffer },
    CardComplexity.Advanced,
    CardTemple.Nature,
    "It allows you to draw two more cards",
    bloodCost: 1,
    hideAttackAndHealth: true,
    defaultTex: AssetHelper.LoadTexture("kettle_of_avarice"),
    specialStatIcon: GlobalSpellAbility.Instance.statIconInfo.iconType,
    specialAbilitiesIdsParam: new List<SpecialAbilityIdentifier>() { GlobalSpellAbility.Instance.id },
    abilityIdsParam: new List<AbilityIdentifier>() { DrawTwoCards.Identifier }
);

NewCard.Add(
    "Lightning",
    "Lightning",
    0, 0,
    new List<CardMetaCategory>() { CardMetaCategory.ChoiceNode, CardMetaCategory.TraderOffer },
    CardComplexity.Advanced,
    CardTemple.Nature,
    "A perfectly serviceable amount of damage",
    bloodCost: 1,
    hideAttackAndHealth: true,
    defaultTex: AssetHelper.LoadTexture("lightning_bolt"),
    specialStatIcon: TargetedSpellAbility.Instance.statIconInfo.iconType,
    specialAbilitiesIdsParam: new List<SpecialAbilityIdentifier>() { TargetedSpellAbility.Instance.id },
    abilityIdsParam: new List<AbilityIdentifier>() { DirectDamage.Identifier, DirectDamage.Identifier }
);
```

## Adding a spell through JSON Loader

To add a spell using JSON loader, you need to know the GUID and name of the special ability. See below for an example:

```
{
  "name": "Kettle_of_Avarice",
  "displayedName": "Kettle of Avarice",
  "description": "It allows you to draw two more cards",
  "metaCategories": [
    "ChoiceNode",
    "TraderOffer"
  ],
  "cardComplexity": "Advanced",
  "temple": "Nature",
  "baseAttack": 0,
  "baseHealth": 0,
  "bloodCost": 1,
  "customAbilities": [
    {
      "name": "Draw Twice",
      "GUID": "zorro.infiniscryption.sigils.drawtwocards"
    }
  ],
  "texture": "kettle_of_avarice.png",
  "customSpecialAbilities": [
	{
      "name": "Spell (Global)",
      "GUID": "zorro.infiniscryption.sigils.globalspell"
	}
  ]
}

{
  "name": "Lightning",
  "displayedName": "Lightning",
  "description": "A perfectly serviceable amount of damage",
  "metaCategories": [
    "ChoiceNode",
    "TraderOffer"
  ],
  "cardComplexity": "Advanced",
  "temple": "Nature",
  "baseAttack": 0,
  "baseHealth": 0,
  "bloodCost": 1,
  "customAbilities": [
    {
      "name": "Direct Damage",
      "GUID": "zorro.infiniscryption.sigils.directdamage"
    }
  ],
  "texture": "lightning_Bolt.png",
  "customSpecialAbilities": [
	{
      "name": "Spell (Targeted)",
      "GUID": "zorro.infiniscryption.sigils.targetedspell"
	}
  ]
}
```

## What sigils are in this pack?

So far there are two, with some more to come:

- **Draw Twice** (GUID: "zorro.infiniscryption.sigils.drawtwocards"): Draw the top card of your main deck and the top card of your side deck when this card dies.
- **Cataclysm** (GUID: "zorro.infiniscryption.sigils.cataclysm"): Kill every card on board when this card dies.
- **Direct Damage** (GUID: "zorro.infiniscryption.sigils.directdamage"): Deals one damage to the targeted card slot. This ability stacks, so if you put two on a card, it will deal two damage.

## Changelog

<details>
<summary>Changelog</summary>

1.1
- Added support for targeted spells.
- Fixed card animations

1.0
- Initial version. Adds global spells.
</details>