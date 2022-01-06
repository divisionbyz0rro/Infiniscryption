# P03 Kaycee's Mod

This mod is in an alpha state! You should back up your save file before running it; I'm about 90% sure it won't break your existing Act 3 save, but I'm definitely not 100% sure of that.

Also, *please* turn on logging before running this mod. Open up your BepInEx.cfg file and set the following:

```
[Logging.Disk]

WriteUnityLog = true

AppendLog = false

Enabled = true

LogLevels = All
```

## What does this mod do:
- Brings P03 gameplay into Kaycee's Mod. There is now a separate 'New Run' button on the main screen that will start a P03 run instead of a Leshy run.
- P03 runs use procedurally generated maps. You will start on a hub space with a draft node (*please* remember to draft), and you can choose which of the four regions to play through. Once you enter a region, you're stuck there until you complete it.
- Each region has four enemy encounters. You must clear these four encounters to be able to enter the boss chamber.
    - Entering the boss chamber will automatically start the boss encounter, and defeating the boss will automatically send you back to the hub region (and you will no longer be allowed to visit that region).
- Each region has regional upgrades in addition to standard upgrades:
    - Undead: Overclock
    - Nature: Add beast transformation
    - Wizard: Gemify card (entering this zone automatically gemifies your side deck)
    - Tech: Build a card (the start of this zone will also allow you to add conduits to your side deck)
- There are currently four starting decks to choose from. They're not necessarily super well-balanced (yet). More will be built.
- Some of the challenges work; most don't. However, their costs will still be applied. The following challenges will work:
    - Pricey Pelts (will make upgrades cost more money)
    - Tipped Scales
    - More Difficult
    - Single Candle

## What is on the to-do list?
- Incorporate Act 3 items into the game. Right now there is no way to get items at all. The plan is to start you with items based on challenge level and then allow you to rebuy them at points in the map.
- Conduits are lame. The tech zone lets you conduit-ify your side deck, but you're not ever going to have enough conduits to make a real conduit deck. I need to work through this somehow.
- Fix the remaining challenges and properly calculate challenge level.
- Investigate the Overclock upgrade. Right now I'm not convinced there's any reason to ever do this because it costs you money. If it was free, you might consider it as something analagous to the fires from a Leshy run; either you make a bad card okay, or you kill the bad card. But these P03 runs are different with regards to how you curate your deck. Right now I'm considering a consolation prize; if an overclocked card dies, you get a draft token added to your deck.
- Increase the card pool by bringing over the Act 2 Tech cards that are not currently available in Act 3.
- This will also end up bringing in some Rare cards. My plan right now is to add a Rare Draft Token and give you one each time you beat a boss as a way to re-implement the rare card selection event from Act 1.
- Some events are currently not in this mod. Forced card trades, card recycling, and damage races are currently not incorporated, and I need to figure out the right way to do that.
- Rework the four bosses. The bosses need varying amounts of work; Canvas and Photographer are pretty much fine as is - they just need some difficulty tweaks. Golly needs to be reworked to not rely on an internet connection (as much as is possible) and the second phase needs to be replaced with something more challenging than "just build an instant-win card". The Archivist needs the most amount of work - he's incredibly uninteresting to play against.
- Build a final boss. You will eventually fight P03 directly as the final boss of this. I have some ideas of what this encounter will be, but there's a lot of work to do to get there.

## Known Issues
- Landmarks sometimes sit on top of upgrades. You can still see and use the upgrades, but it looks weird.
- Bosses don't recognize when they've completed story events. For example: the Archivist will ask you for permission to access your hard drive every turn. I will fix this when I start working on making bosses better.
- The active gem displayer does not show up during battle. Gems work just fine, but the little gem displayer module is not attached to the energy module.
- One of the gem rooms in the Wizard zone puts landmarks in the very middle of the room and needs to be fixed.
- The forest zone is underlit and some cards don't display correctly as a result.
- The tech zone just generally looks weird and some landmarks don't display correctly.


## Requirements

As with most mods, you need [BepInEx](https://inscryption.thunderstore.io/package/BepInEx/BepInExPack_Inscryption/) installed. 

You will also need the [API](https://inscryption.thunderstore.io/package/API_dev/API/) installed.