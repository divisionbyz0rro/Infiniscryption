# Curses!

Now refactored for Kaycee's Mod: this mod adds a number of new Challenges and Assists that you can select from when starting a new run.

The following challenges are added:

- **Haunted Past**: Deathcards will sometimes attack you in battle
- **Chaotic Enemies**: Enemies will gain additional random sigils in battle
- **Boss Revenge**: Each boss gains a third phase

The following assists are added:

- **Extra Candle**: You gain an extra life
- **Golden Beginnings**: You start the game with a golden pelt in your deck

## Requirements

As with most mods, you need [BepInEx](https://inscryption.thunderstore.io/package/BepInEx/BepInExPack_Inscryption/) installed. You also need the [API](https://inscryption.thunderstore.io/package/API_dev/API/)

## Installation

The zip file should be structured in the same way as your Inscryption root directory. Just drop the 'BepInEx' folder into your Inscryption directory and you're golden.

## Changelog

<details>
<summary>Changelog</summary>

1.0
- Completely written to be compatible with Kaycee's Mod.
- Curse manager has been removed from the this mod and ported over to the API as the Challenge Manager.

0.4.3
- Fixed texture defect by ensuring that all textures are loaded up front (prevents occasional crash when spell cards show up in the wrong order).

0.4.2
- Fixed defect where deathcards were being selected using the wrong random seed.

0.4.1
- Fixed defect with multiple attacks and mega sharks

0.4.0
- Added the Curse of the Wilting Clover
- Added the Curse of Boss' Revenge

0.3.1
- Fixed defect where unpaused audio would sometimes come back too loud.

0.3.0
- Added 'Curse of Loose Teeth' and 'Curse of Chaos'
- Fixed defect where audio would sometimes not properly resume after deathcards appear in battle
- Improved how 'Curse of the Lone Candle' interacts with boss sequences.
- Curse selection now happens after drafting/starter deck selection and after side deck selection

0.2.1
- Fixed occasional crash when adding deathcards to battle plan
- Fixed random icons appearing on procedurally generated cards.

0.2.0
- Added the 'Curse of Haunted Pasts'
- Significantly improved the curse selection event
- Fixed a bug in the curse selection event where card backs were not displaying correctly.

0.1.1
- Updated asset loader to (hopefully) fix issues with Thunderstore mod manager.

0.1.0
- Initial version. Includes 'Curse of the Empty Backpack,' 'Curse of the Lone Candle', ' and 'Curse of the Empty Backpack'
</details>