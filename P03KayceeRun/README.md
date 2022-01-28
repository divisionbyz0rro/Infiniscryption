# P03 Kaycee's Mod

This mod is in a beta state! You should back up your save file before running it; I'm about 99% sure it won't break your existing Act 3 save, but that's not 100%.

Also, *please* turn on logging before running this mod. Open up your BepInEx.cfg file and set the following:

```
[Logging.Disk]

WriteUnityLog = true

AppendLog = false

Enabled = true

LogLevels = All
```

## Feedback

Please feel free to give me feedback on the Inscryption modding discord - @divisionbyzorro

## How does this mod work?

When you install this mod, you will see two 'new run' options on the opening screen for Kaycee's Mod. You can either start a New Leshy Run (the default KCM experience) or a new P03 run (this mod's experience).

P03 runs differ from Leshy runs in a few significant ways:

1) You have more choice over how the maps play out:
    - You can fight through the four zones in any order you choose. 
    - You are not forced to use any map nodes that you don't want to. (Pro tip: you still need to add cards to your deck - matches last longer than you might be used to).
2) Most of your choices are based on how you manage your currency. You will find currency on the map, and you earn it by 'overkilling' P03 during battles. (Pro tip: you should deliberately overkill as much as possible if you want to build a strong of a deck as possible). Every upgrade you select will cost you robobucks. The only thing that's free is adding new cards to your deck.

There are also some changes from the way P03's gameplay worked the first time through:

1) Bosses have been updated. G0lly and the Archivist are the most significantly different; both of them have completely reworked second phases. 
2) There is a final boss fight against P03. He has...some thoughts about what you're doing.
3) Some events work differently (see below).

However the runs are still similar in significant ways:

1) You will still start with a draft. **Warning:** The game will not force you to complete the draft - don't forget to click that purple draft node whenever you're back at the hub!
2) There are a number of starter decks that you can pick from to help guide your selections through the rest of the game.
3) You must complete four battles against enemies before facing the boss. Once you beat the boss, you're done with that zone and can't go back.
4) There are now rare cards available to you. You will be given a Rare Token for completing a boss, which you can spend at the draft node to get a rare for your deck.

### Events

Some events play the same in this mod as they did the first time you played through Botopia. However, some are different. Here's what you need to know before spending your robobucks on an event:

- **Build-a-Card**: This is mostly the same as before, but the ability pool has been modified. The Unkillable sigil is no longer able to be added to a build-a-card, and a few new abilities (such as conduits and cells) are added.
- **Gemify Cards**: This behaves the same as before.
- **Items**: You can buy an item like you would in Act 1. However, you can only get one item from each shop.
- **Overclock**: This is significantly different. As before, the overclocked card gets +1 attack. However, when an overclocked card dies, it is not just removed from your deck. It is replaced with an Exeskeleton with the same set of abilities as the original card. So if you overclock a Sniper and it dies, you will get an Exeskeleton with the Sniper sigil.
- **Recycle**: Instead of getting robobucks back for your recycled card, you get a draft token. Normal cards get you a standard Token. Cards that have been upgraded at least once give you an Improved Token. Rare cards get you a Rare token.
- **Transformer**: Currently, this behaves as before - however, this is actively under development.

### Challenges

Some challenges simply don't work in this context. Any challenge that doesn't work will be 'locked' and you won't be able to select it.

## What is on the to-do list?
- Figure out how to make the Beast Transformer event not lame.
- Add more rare cards
- Figure out how to balance the Energy Conduit - this thing is ridiculous

## Known Issues
- Canvas' face doesn't display right. I'm trying to streamline the bosses, so I'm skipping the "choose face" part of the boss fight, but something is preventing a randomly generated face from showing.

## Credits

The pixel/GBC card arts are taken from the [Act II Recreated](https://inscryption.thunderstore.io/package/Sire/RecreatedAct2Cards/) Mod, with art by Sire, SyntaxEvasion, ExtraOrdiNora, TheGreenDigi and DragonSlayr.

## Requirements

As with most mods, you need [BepInEx](https://inscryption.thunderstore.io/package/BepInEx/BepInExPack_Inscryption/) installed. 

You will also need the [API](https://inscryption.thunderstore.io/package/API_dev/API/) installed.