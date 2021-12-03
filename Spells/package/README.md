# Spell Cards

This mod contains special abilities that allow you to create a new type of card called a 'spell.' Spells are cards that

a) Do not need a space on board to resolve
b) Die immediately when played.

In other words, these are cards you play entirely because they have an immediate effect.

There are also some additional sigils in this pack that might be useful for you when creating spells.

## Does this pack add any cards?

It can, but it doesn't by default. If you want my example cards added to the card pool, go to the config file 'zorro.inscryption.infiniscryption.spells.cfg' and set 'AddCards' to true.

This will add the following cards:

- **Kettle of Avarice**: 1 blood, draws two cards.
- **Anger of the Gods**: 1 blood, destroys all creatures on board (Rare).

## Future enhancements:

1. Targeted spells: Targeted spells will allow you to target a space on the board for the spell to have an effect.
2. More sigils: More sigils will be added to the pack to help you piece together new cards.

## Requirements

As with most mods, you need [BepInEx](https://inscryption.thunderstore.io/package/BepInEx/BepInExPack_Inscryption/) installed. 

You will also need the [API](https://inscryption.thunderstore.io/package/API_dev/API/) installed.

## Adding a spell through the API

To add a spell using the API, you need to add both a 'special ability' and a 'stat icon'. Also, make sure you add a sigil that triggers either when played from hand or when the card dies. Otherwise nothing will happen when the card is played.

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
```

## Wait, isn't there another mod that does something like this?

There are a lot of ways to accomplish the same thing. This method is modular, easy to use, gives spells a custom card frame, and most importantly it doesn't waste a sigil slot on the card.

## What sigils are in this pack?

So far there are two, with some more to come:

- **Draw Twice** (GUID: "zorro.infiniscryption.sigils.drawtwocards"): Draw the top card of your main deck and the top card of your side deck when this card dies.
- **Cataclysm** (GUID: "zorro.infiniscryption.sigils.cataclysm"): Kill every card on board when this card dies.