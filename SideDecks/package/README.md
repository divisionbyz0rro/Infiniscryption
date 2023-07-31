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

There's also support to allow you or others to add more sidedeck cards to the pool; see below.

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
myCard.SetExtendedProperty("SideDeckValue", 5);
```

If you are using JSON Loader:

```json
{
    "name": "MyCard",
    "metaCategories": [ "zorro.inscryption.infiniscryption.sidedecks.SideDeck" ],
    "extensionProperties": {
        "SideDeckValue": "5"
    }
}
```

## I created a side deck card using a previous version of this mod that used a specific trait number. What now?

Don't worry. That is still supported - for now. However, compatibility for this will be removed at some point in the future. You need to transition away from using that specific trait and over to using this new card metacategory as soon as possible.

## I'm a modder and I want to know which side deck card was selected, but I don't want to enforce a dependency on this mod

The selected side deck card is stored in the modded save file. The best way to reference this value is by creating a static property like so:

```c#
using InscryptionAPI.Saves;

public static string SelectedSideDeck
{
    get 
    { 
        string sideDeck = ModdedSaveManager.SaveData.GetValue("zorro.inscryption.infiniscryption.sidedecks", "SideDeck.SelectedDeck");
        if (String.IsNullOrEmpty(sideDeck))
            return "Squirrel"; // or whatever other default is appropriate for your use case

        return sideDeck; 
    }
}

```

## How does this interact with other mods?

**Leshy Run (default KCM experience)**: Whichever side deck card you select will replace the deck of Squirrels.

**Grimora Mod**: Behaves the same as a standard Leshy run. The side deck pile will be replaced with whatever side deck card you select.

**P03 in Kaycee's Mod**: You will still have a side deck of empty vessels - no matter what. Only the abilities on the selected side deck card matter. The abilities on the side deck card you choose will be added to the empty vessels.

**Magnificus Mod**: This mod is currently not completely supported by the Side Deck Selector mod. By this I mean that the user interface supports Magnificus, but there are no in-game patches to make the selected side deck card actually work. So you can add side deck cards for Magnificus, and you can select them, and the mod will record the selected card, but nothing will actually change during the run as of right now.

## I built a challenge that affects the side deck, but this mod is overriding it. Help!

Good news! You can now connect to an event in the `SideDeckManager` to override the list of available side deck cards. This event requires two parameters. The first parameter, a `CardTemple`, is an indicator as to which region the side deck is in (for example, `CardTemple.Tech` refers to P03 Mod, `CardTemple.Undead` refers to Grimora Mod, etc). The second parameter, a `List<string>`, is the current set of side deck cards. Modify them as necessary!

```c#
using Infiniscryption.SideDecks.Patchers;

SideDeckManager.ModifyValidSideDeckCards += delegate(CardTemple temple, List<string> sideDeckCards)
{
    if (temple == CardTemple.Undead && AscensionSaveData.Data.ChallengeIsActive(AscensionChallenge.SubmergeSquirrels))
    {
        sideDeckCards.Clear(); // CAREFUL: If you want to completely replace the list, you must modify it in-place by clearing it
        sideDeckCards.Add("MyMod_GoofySkeleton");
    }
};
```

## Changelog 

<details>
<summary>Changelog</summary>

2.1.5
- I'm back. Deal with it.
- Made the documentation better.
- Added ability for mods to modify the side deck list; the primary use case here is to enable different types of challenges.
- Both Grimora Mod and P03 Mod should now be fully supported. Magnificus Mod will require more work/collaboration.
- KNOWN ISSUE: The side deck modification node (enabled by an optional green challenge) for Leshy runs is still using the deprecated method for adding new nodes to the map.

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