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

This mod would not be possible without signifcant contributions from the Inscryption Modding discord channel.

Pixel icons were contributed by [Arakulele](https://inscryption.thunderstore.io/package/Arackulele/).

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

- **Draw Twice** ("zorro.inscryption.infiniscryption.spells.Draw Twice"): Draw the top card of your main deck and the top card of your side deck when this card dies.
- **Cataclysm** ("zorro.inscryption.infiniscryption.spells.Cataclysm"): Kill every card on board when this card dies.
- **Direct Damage** ("zorro.inscryption.infiniscryption.spells.Direct Damage"): Deals one damage to the targeted card slot. This ability stacks, so if you put two on a card, it will deal two damage. This only targets opponent cards.
- **Direct Healing** ("zorro.inscryption.infiniscryption.spells.Direct Heal"): Heals the targeted card for one. This can overheal. This ability stacks. This only targets player cards.
- **Attack Up** ("zorro.inscryption.infiniscryption.spells.Attack Up"): Increases the targeted card's attack by one for the rest of the batle. This only targets player cards.
- **Attack Down** ("zorro.inscryption.infiniscryption.spells.Attack Down"): Decreases the targeted card's attack by one for the rest of the batle. This only targets opponent cards.
- **Gain Control** ("zorro.inscryption.infiniscryption.spells.Gain Control"): Gains control of the targeted creature, but only if there is an empy slot for that creature to move into. Functionally similar to the fishook item.

## Split, Tri, and All Strike
These sigils do **nothing** for global spells, but behave as you would expect for targeted spells. Be careful when putting Split Strike on a targeted spell, as it will behave exactly as expected, which is not necessarily intuitive. Rather than affecting the targeted space, it will affect the spaces on either side.

The spell will trigger once for each targeted slot, but only the SlotTargetedForAttack and ResolveOnBoard triggers will fire multiple times. The PlayFromHand and Die triggers will only happen once.

So, for example:
- A spell with All Strike and Create Dams will attempt to put a beaver dam in every space on the boad.
- A spell with Tri Strike and Direct Damage will deal one damage to the targeted space and both adjacent spaces.
- A spell with Split Strike and Explode On Death will only explode once.

## Adding a spell through the API

The best way to add a spell using the API is to also create a reference to this mod's DLL in your project and use the custom extension method helpers "SetGlobalSpell()" or "SetTargetedSpell()" to turn a card into a spell:

```c#
using InscryptionAPI;
using Infiniscryption.Spells.Sigils;

CardManager.New("Spell_Kettle_of_Avarice", "Kettle of Avarice", 0, 0, "It allows you to draw two more cards")
           .SetDefaultPart1Card()
           .SetPortrait(AssetHelper.LoadTexture("kettle_of_avarice"))
           .SetGlobalSpell() // Makes this card into a global spell
           .SetCost(bloodCost: 1)
           .AddAbilities(DrawTwoCards.AbilityID);

CardManager.New("Spell_Lightning", "Lightning", 0, 0, "A perfectly serviceable amount of damage")
           .SetDefaultPart1Card()
           .SetPortrait(AssetHelper.LoadTexture("lightning_bolt"))
           .SetTargetedSpell() // Makes this card into a targeted spell
           .SetCost(bloodCost: 1)
           .AddAbilities(DirectDamage.AbilityID, DirectDamage.AbilityID);
```

## Adding a spell through JSON Loader

To add a spell using JSON loader, you simply need to add either the global spell or the targeted spell special ability to the card:

```json
{
  "specialAbilities": [ "zorro.inscryption.infiniscryption.spells.Spell (Global)" ]
}

{
  "specialAbilities": [ "zorro.inscryption.infiniscryption.spells.Spell (Targeted)" ]
}
```

## Changelog

<details>
<summary>Changelog</summary>

2.0
- Updated documentation for Kaycee's Mond API and required that API as a dependency.

1.2.7
- Added pixel icons for compatibility with GBC mode

1.2.6
- Prevented the game from soft locking if you back out of casting a spell partway through sacrificing creatures.

1.2.5
- Fixed texture loading defect to prevent crashes when spell cards appear in certain situations for the first time.
- Updated mod to have a dependency on the unofficial patch as opposed to the standalone visually stackable sigils mod.

1.2.4
- Added the fishhook sigil

1.2.3
- Bad manifest.json. My bad. :(

1.2.2
- Updated to be dependent on the Stackable Sigils mod. This makes spell creation with modular sigils far more user friendly.

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