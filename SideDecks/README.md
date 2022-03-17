# Side Deck Selector

Are you tired of those boring old squirrels? Have I got the thing for you...

This mod comes with seven different side decks to select from:
- **Squirrels:** Now with the ability to block fliers!
- **Bees:** You don't even need to find the bee totem anymore - but they do have Brittle now.
- **Ants:** Want some help making those ants a little better? These 1/Ant helpers will buff your little ant army for free.
- **Squids:** These squishy and slippery things have no bones, but are really hard to pin down.
- **Puppies:** Adorable. Energetic. They move from place to place and dig up a fresh bone for you every turn.
- **Goats:** They generate twice the blood when they die, but they knock out two of your teeth for the trouble.
- **Amalgam Eggs:** Did you know that the Amalgam was hatched from an egg? These eggs somehow belong to every tribe in the game at once and as such will always gain the ability from your totem.

There's also a hook to allow you or others to add more sidedeck cards to the pool; see below.

## Requirements

- [BepInEx](https://inscryption.thunderstore.io/package/BepInEx/BepInExPack_Inscryption/)
- [API](https://inscryption.thunderstore.io/package/API_dev/API/)
- [HealthForAnts](https://inscryption.thunderstore.io/package/JulianMods/HealthForAnts/)

## Installation

The zip file should be structured in the same way as your Inscryption root directory. Just drop the 'BepInEx' folder into your Inscryption directory and you're golden.

## I want to develop a new starter deck card and use it here. How do I do that?

You need to add the "side deck" metacategory to your card. You can also control how many challenge points the player will lose by choosing your side deck card by setting the "SideDeckValue" extended property to the appropriate number of points (by default, the player will not lose any points by choosing a side deck card, but if your card is better than a squirrel, the player should probably get some points taken off of their challenge rating if they choose it).

```c#
public static readonly CardMetaCategory SIDE_DECK_CATEGORY = GuidManager.GetEnumValue<CardMetaCategory>("zorro.inscryption.infiniscryption.sidedecks", "SideDeck");

// sometime later...
CardInfo myCard = ...;
myCard.AddMetaCategories(SIDE_DECK_CATEGORY);
myCard.SetExtendedPropert("SideDeckValue", 5);
```

If you are using JSON Loader:

```json
{
    "name": "MyCard",
    "metaCategories": ["zorro.inscryption.infiniscryption.sidedecks.SideDeck"],
    "extensionProperties": {
        "SideDeckValue": "5"
    }
}
```

## I created a side deck card using a previous version of this mod that used a specific trait number. What now?

Don't worry. That is still supported - for now. However, compatibility for this will be removed at some point in the future. You need to transition away from using that specific trait and over to using this new card metacategory as soon as possible.

## Changelog 

<details>
<summary>Changelog</summary>

2.0
- Added compatibility with Kaycee's Mod
- Added the Amalgam Egg
- Switched from traits to metacategories

1.2
- Changed the name of the ant sidedeck creature
- Made the tentacle creature into an 0/2 to make it actually playable

1.1.1
- Fixed defect in Gelatinous ability that causes it to crash the game when bones are added to the pool from any source other than a card dying.

1.1
- Added hooks to allow additional cards to be added to the starter card pool.

1.0
- Initial version. Adds the sidedeck selection node and six possible side decks into the pool.
</details>

## Acknowledgements

Thanks to everyone on the Inscryption modding discord for all of their feedback and ideas on starter deck cards. I would love ideas for more decks and feedback on these cards - ping me on the discord @divisionbyzorro.