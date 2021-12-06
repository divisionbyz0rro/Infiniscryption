# Spell Cards

This mod contains special abilities that allow you to create a new type of card called a 'spell.' Spells are cards that

- Do not need a space on board to resolve
- Die immediately when played.

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
- **Rot Healing**: 1 bone, heals for two
- **Irritate**: 2 bones, does one damage but increases attack by one
- **Go Fetch**: Free to cast, generates 4 bones when cast
- **Compost**: 3 bones, draws two cards.

These cards are not meant to be balanced, but rather to demonstrate how the mod works (hence why they are not added by default).

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

## Target selection for targeted cards

When it comes time to select a target for a targeted card, the game will ask the card if it responds to being targeted at that card slot using the 'RespondsToSlotTargetedForAttack' override. It will only allow you to target a spell at a slot if the card says it will respond to being pointed at that slot. Additionally, if the card responds to 'ResolveOnBoard,' the game will allow you to target *any* of the player's four slots.

The sigils included in this pack are built so that the ones that do harm will respond 'false' when pointed at a player card and 'true' when pointed at an opposing card (and vice versa for sigils that are beneficial). Note that this means if you combine a positive and negative effect on the same card (for example, a spell that increases attack by one but also damages the card for one), that card will be able to target both friendly and enemy cards.

If you are adding sigils that are intended to be used on targeted spells, you need to make sure that the 'RespondsToSlotTargetedForAttack' correctly identifies if this sigil should be applied to that slot.

## What sigils are in this pack?

So far we have the following:

- **Draw Twice** (GUID: "zorro.infiniscryption.sigils.drawtwocards"): Draw the top card of your main deck and the top card of your side deck when this card dies.
- **Cataclysm** (GUID: "zorro.infiniscryption.sigils.cataclysm"): Kill every card on board when this card dies.
- **Direct Damage** (GUID: "zorro.infiniscryption.sigils.directdamage"): Deals one damage to the targeted card slot. This ability stacks, so if you put two on a card, it will deal two damage. This only targets opponent cards.
- **Direct Healing** (GUID: "zorro.infiniscryption.sigils.directheal"): Heals the targeted card for one. This can overheal. This ability stacks. This only targets player cards.
- **Attack Up** (GUID: "zorro.infiniscryption.sigils.attackup"): Increases the targeted card's attack by one for the rest of the batle. This only targets player cards.
- **Attack Down** (GUID: "zorro.infiniscryption.sigils.attackdown"): Decreases the targeted card's attack by one for the rest of the batle. This only targets opponent cards.

## Split, Tri, and All Strike
These sigils do **nothing** for global spells, but behave as you would expect for targeted spells. Be careful when putting Split Strike on a targeted spell, as it will behave exactly as expected, which is not necessarily intuitive. Rather than affecting the targeted space, it will affect the spaces on either side.

The spell will trigger once for each targeted slot, but only the SlotTargetedForAttack and ResolveOnBoard triggers will fire multiple times. The PlayFromHand and Die triggers will only happen once.

So, for example:
- A spell with All Strike and Create Dams will attempt to put a beaver dam in every space on the boad.
- A spell with Tri Strike and Direct Damage will deal one damage to the targeted space and both adjacent spaces.
- A spell with Split Strike and Explode On Death will only explode once.

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
## A note on stackable sigils

As of this release, the API has a defect that will not allow custom sigils to stack. This mod includes a fix for that defect, but will only patch versions of the API below 1.12.1.

## Changelog

<details>
<summary>Changelog</summary>
1.2.1
- Fixed defect with Attack Up and Attack Down where they were not properly attaching to cards.
- Fixed defect where sometimes creatures could not be played after casting targeted spells.
- Added more example cards to the pool.

1.2
- Added targeting logic for targeting spells. They will now only allow you to select valid targets.
- Added support for split strike, tri strike, and all strike
- Added modular, stackable sigils for spell creation.

1.1
- Added support for targeted spells.
- Fixed card animations

1.0
- Initial version. Adds global spells.
</details>