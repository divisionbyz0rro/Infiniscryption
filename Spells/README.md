# Spell Cards

This mod contains special abilities that allow you to create a new type of card called a 'spell.' Spells are cards that

a) Do not need a space on board to resolve
b) Die immediately when played.

In other words, these are cards you play entirely because they have an immediate effect.

There are also some additional sigils in this pack that might be useful for you when creating spells.

## Future enhancements:

1. Targeted spells: Targeted spells will allow you to target a space on the board for the spell to have an effect.
2. More sigils: More sigils will be added to the pack to help you piece together new cards.

## Requirements

As with most mods, you need [BepInEx](https://inscryption.thunderstore.io/package/BepInEx/BepInExPack_Inscryption/) installed. 

You will also need the [API](https://inscryption.thunderstore.io/package/API_dev/API/) installed.

## Adding a spell through the API

To add a spell using the API, you need to add both a 'special ability' and a 'stat icon'. Also, make sure you add a sigil that triggers either when played from hand or when the card dies.

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

There are a lot of ways to accomplish the same thing. This method is modular, easy to use, and most importantly it doesn't waste a sigil slot on the card and doesn't put a sigil in the pool that does nothing on its own.