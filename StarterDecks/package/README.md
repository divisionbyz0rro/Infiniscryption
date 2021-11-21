# Upgradable Starter Decks Mod

**It would be a shame to let perfectly good teeth go to waste...**

This Act I mod allows you to select from three different theme decks at the start of the map (Wolves, Birds, and Elks) rather than being stuck with the same default starter deck every time.

Additionally, any teeth leftover at the end of a run are saved by Leshy, as well as some extra bonus teeth. You can then spend those teeth to upgrade the quality of the starter decks. Upgrades are accessed by clicking the teeth in the skull in the cabin. Instead of giving away free teeth, it now takes you to a cusom upgrade store.

## Requirements

As with most mods, you need [BepInEx](https://inscryption.thunderstore.io/package/BepInEx/BepInExPack_Inscryption/) installed. There are no other requirements.

## Conflicts

Because this modifies your starter deck, it conflicts with the (excellent) [Drafting Mod](https://inscryption.thunderstore.io/package/PortaMods/DraftingMod/).

Version 1.0.0 is incompatible with the [Extended Maps Mod](https://inscryption.thunderstore.io/package/Cyantist/ExtendedMaps/). Versions 1.0.1 and above have no conflicts.

## Starter Decks

There are three starter decks to choose from:

- **Wolves**: Wolf, Wolf Cub, Coyote, Bullfrog
- **Birds**: Sparrow, Bat, Raven Egg, River Snapper
- **Elks**: Elk, Fawn, Pronghorn, Porcupine

Each deck has eight predefined upgrades. The first upgrade costs 4 teeth, then 8, then 12, etc. Once you get the 8 defined upgrades, the cards continue to upgrade their health, then attack, alternating indefinitely. 

Yes, this means that if you collect enough teeth you'll have a deck of 1-cost one-shot cards. But the amount of teeth required to do that gets absurd (barring Oroborous abuse).

## Technical Stuff
This is technical implementation details you probably don't care about.

<details>
<summary>Changelog</summary>

1.0.1
- Fixed unexpected incompatibility with the [Extended Maps Mod](https://inscryption.thunderstore.io/package/Cyantist/ExtendedMaps/).
</details>

<details>
<summary>Configuration</summary>

The starter decks, deck evolutions, and cost to upgrade decks are all configurable. However, configuration is locked when the mod starts for the first time. You have to wipe your save (or use Chapter Select to restart Part 1) if you want configuration changes to take hold (or modify the save file)
</details>

<details>
<summary>Where data is saved</summary>

All data in your save file is stored in the 'introducedConsumables' list. You can search for 'infiniscryption' in the save file and you will list a list of key/value pairs, stored as 'key=value'. If you what comes after the '=' sign, be careful to follow the same pattern. And don't cheat to give yourself 10000 teeth; the decks will literally upgrade forever, and you'll end up with a deck full of one-shot wins. :)
</details>

<details>
<summary>Deck upgrade language</summary>

The upgrade paths for decks is stored in configuration (and the save file). They use a 'language' to describe how decks upgrade which allows the ugprade path to fit on a single line of text. Here is an example;

1=Wolf_Talking,3+2H&+WhackAMoleS,2=Alpha&+-1O,0+TailOnHitS,3+SharpS,2+-1O&+1H,0+DrawRabbitsS,1+FlyingS

This is a comma-separated list of ugprade instructions. Each instruction starts with a number (0-3) indicating which card gets upgraded. The second character tells if the card is getting replaced ('=') or improved ('+'). Multiple upgrades can happen at a time, using the '&' character.

- Replacement: Just list the card key name (e.g., 'WolfCub').
- Upgrade: The *last* character tells what you're changing. You can change blood cost ('B'), bone cost ('O'), attack ('A'), health ('H'), or add a sigil ('S').

So for example, if we want the first card in the deck to gain the 'Flying' sigil but lose one health, the command would be '1+FlyingS&+-1H'. Note that to lose a health the command still starts with a '+', because we are adding a modification. The modification has a '-1H' on it.
</details>

<details>
<summary>Cutscenes</summary>

Some of the game's cut scenes have been modified. Look in 'MetaCurrency_GainTeeth' for most of the code that does this.
</details>