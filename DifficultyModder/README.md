# Curses!

Now refactored for Kaycee's Mod: this mod adds a number of new Challenges and Assists that you can select from when starting a new run.

If you're used the old version of this mod that adds an event to the map in Part 1,that event no longer exists. This mod is now exclusive to Kaycee's Mod. If you wanted the old functionality, you'll have to install an old version of this mod.

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

## Requirements

As with most mods, you need [BepInEx](https://inscryption.thunderstore.io/package/BepInEx/BepInExPack_Inscryption/) installed. You also need the [API](https://inscryption.thunderstore.io/package/API_dev/API/)

## Installation

The zip file should be structured in the same way as your Inscryption root directory. Just drop the 'BepInEx' folder into your Inscryption directory and you're golden.

## A Personal Message from DivisionByZ0rro (7/19/2022)

It's been a while since you've heard from me. Life changes quickly. I got a bad case of Covid, I had family members get seriously injured, and was just generally unavailable for a while. 

Working on this and other Inscryption mods has been an amazing collaborative journey over the past months. Ever since I completed Inscryption for the first time in the fall of 2021, I spent all of my spare time (and then some) working on modding this game and being a part of an incredible community. But unfortunately, things change, and I cannot keep this up moving forward. I simply don't have the same amount of spare time that I used to, and it's time for me to move on.

I have nothing but gratitude for everyone who supported me and helped me accomplish what I have been able to accomplish. I know I'm leaving work unfinished, but I know that would be true no matter when I called it quits.

If anybody wants to continue any of my work, I hereby grant unrestricted permission for anyone to fork any of projects and make it their own moving forward. This work was always a passion project for the community, and I would be honored if anyone on the community wanted to continue that work. Please feel free to copy anything I've done and use it for yourself.

Thanks for everything,
/0

## Changelog

<details>
<summary>Changelog</summary>

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