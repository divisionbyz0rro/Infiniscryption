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

## A Personal Message from DivisionByZ0rro (7/18/2022)

It's been a while since you've heard from me. Life changes quickly. I got a bad case of Covid, I had family members get seriously injured, and was just generally unavailable for a while. 

Working on this and other Inscryption mods has been an amazing collaborative journey over the past months. Ever since I completed Inscryption for the first time in the fall of 2021, I spent all of my spare time (and then some) working on modding this game and being a part of an incredible community. But unfortunately, things change, and I cannot keep this up moving forward. I simply don't have the same amount of spare time that I used to, and it's time for me to move on.

I have nothing but gratitude for everyone who supported me and helped me accomplish what I have been able to accomplish. I know I'm leaving work unfinished, but I know that would be true no matter when I called it quits.

If anybody wants to continue any of my work, I hereby grant unrestricted permission for anyone to fork any of projects and make it their own moving forward. This work was always a passion project for the community, and I would be honored if anyone on the community wanted to continue that work. Please feel free to copy anything I've done and use it for yourself.

Thanks for everything,
/0

## Changelog 

<details>
<summary>Changelog</summary>

2.1.4
- A final message from DivisionByZ0rro

2.1.3
- Compatibility patch for new version of API

2.1.2
- Properly handle the case where the P03 in Kaycee's Mod plugin is installed while the game is in P03 mode.

2.1.1
- The side deck node now only appears on maps 2 and 3.

2.1
- The goat is not broken anymore
- The side deck node now only appears on the map if you activate it via a green challenge

2.0.1
- Fixed defect where the side decks mod was activating P03 side decks while not in Ascension mode
- Set dependency on the proper version of the LifeCost mod

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