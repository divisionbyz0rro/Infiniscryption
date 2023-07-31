# Curses!

This mod adds a number of new Challenges and Assists that you can select from when starting a new run.

The following challenges are added:

- **Haunted Past**: Deathcards will sometimes attack you in battle
- **Chaotic Enemies**: Enemies will gain additional random sigils in battle
- **Boss Revenge**: Each boss gains a new, custom third phase.
- **Bigger Moon**: The moon has more health and attack
- **Two Turn Minimum**: You cannot deal lethal damage on the first turn
- **Dangerous Deck**: After each boss battle, a stick of dynamite is added to your deck

The following assists are added:

- **Extra Candle**: You gain an extra life
- **Golden Beginnings**: You start the game with a golden pelt in your deck
- **Totem Collector**: You start with all tribal totem tops
- **Minor Boon of the Bone Lord**: You start the game with a Minor Boon of the Bone Lord

## Installation

The zip file should be structured in the same way as your Inscryption root directory. Just drop the 'BepInEx' folder into your Inscryption directory and you're golden.

## Changelog

<details>
<summary>Changelog</summary>

1.1.2
- Added 7 achievements using the Achievement API (currently in beta). The beta nature of that API means that there may be some unexpected bugs.

1.1.1
- The Prospector's dynamite boulders are slightly weaker
- Fixed a defect where in some cases the Rare Pelt start assist was broken

1.1
- I'm back. Deal with it.
- Updated to latest version of API.
- Updated dependencies to use the version of Spells maintained by WhistleWind.
- The tribal totems challenge explicitly now only supports base game tribes.
- Starting with a rare pelt will now force the opening trade node to appear even if you have been a coward.
- Added additional dialogue for when deathcards appear.
- The spooky wind that plays when deathcards appear now properly goes away when they do.
- There are generally fewer bugs. I hope.

1.0.9
- A personal message from DivisionByZ0rro

1.0.8
- Improved compatibility with P03 in Kaycee's Mod. 

1.0.7
- Made a couple of the challenges compatible with P03 in Kaycee's Mod
- Small tweak to the deathcard name pool
- Compatibility for API 2.4+

1.0.6
- Whoopsie. Accidentally left a debug flag in that made deathcards always spawn. 

1.0.5
- Whoops. Included fix to make CellTriStrike not lock the game.

1.0.4
- Chaotic Enemies can now be applied twice. You're welcome.
- Deathcards are now randomly generated using a slightly modified version of the Act 3 Bounty Hunter generator instead of from the player's deathcard pool.

1.0.3
- You can no longer sacrifice Dynamite at the altar

1.0.2
- Game no longer softlocks when a match ends with you holding Dynamite and you didn't overkill by more than 2.
- The sharkbite appearance should not apply to all copies of the same card anymore.

1.0.1
- Game no longer softlocks when trying to summon a deathcard inside of Kaycee's Mod.

1.0
- Completely written to be compatible with Kaycee's Mod.
- Curse manager has been removed from the this mod and ported over to the API as the Challenge Manager.

</details>