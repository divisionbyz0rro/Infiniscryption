# Pack Manager

Have you installed too many card mods and want your runs through Kaycee's Mod to be a little more streamlined? This is the mod for you.

This mod adds a screen to the setup of each run through Kaycee's Mod. It allows you to toggle which cards will or will not appear in the upcoming run through the use of 'packs.' Mods that use this API can register themselves as packs, and players can turn each pack on or off as they wish.

This mod uses the API's concept of a "mod prefix" on each card to identify which card belongs to which pack.

[![Screenshot of the Pack Management Screen](https://i.imgur.com/r1qaJop.png)](https://i.imgur.com/DaV9cEo.png)

## What happens when a pack is "turned off?"

When you deactivate a pack for a run through Kaycee's Mod, this mod will temporarily remove all metacategories from all cards in that pack. This will prevent the card from appearing in card choice nodes, trader nodes, rare card selection nodes, etc. However, other references to these cards (such as Evolve or Ice Cube) will remain.

This mod will also try to remove encounters from each region that contain excluded cards. However, most card pack mods don't come with encounters, which means that a lot of pack combinations will result in not having any valid encounters. In this situation, the mod reverts to using the game's default encounters.

## How are packs discovered?

Packs are discovered by looking at the entire card pool and seeing what cards belong to which prefix. Each card is grouped with its prefix, and assigned a Pack based on that prefix. The game will create a default pack art and attempt to create a default description for every pack of cards it discovers, but mod creators can build their own pack descriptions and pack arts as well.

This mod comes with pack definitions and custom art for the following mods:

- [Gareth's Mod](https://inscryption.thunderstore.io/package/Gareth48/GarethMod/)
- [Eri Card Expansion](https://inscryption.thunderstore.io/package/Eri/Eri_Card_Expansion/)
- [Ara Card Expansion](https://inscryption.thunderstore.io/package/Arackulele/AraCardExpansion/)
- [Hallownest Expansion](https://inscryption.thunderstore.io/package/BlindTheBoundDemon/HallownestExpansion/)

## Requirements

As with most mods, you need [BepInEx](https://inscryption.thunderstore.io/package/BepInEx/BepInExPack_Inscryption/) installed. 

You will also need the [API](https://inscryption.thunderstore.io/package/API_dev/API/) installed.

## Installation

The zip file should be structured in the same way as your Inscryption root directory. Just drop the 'BepInEx' folder into your Inscryption directory and you're golden.

# How do I make my cards compatible with the Pack Manager?

## Make sure all of your cards use the same mod prefix
This cannot be stressed enough. This mod relies on the mod prefix to sort out which cards belong to which pack, so it is your responsibility to make sure that you've selected a mod prefix for your cards and that all your cards use it.

## Pack Metacategories

Packs can be belong to any number of "metacategories" which define which scribe they are valid for. As of the time this was written, this will only matter if the player also has my P03 in Kaycee's Mod mod installed. Otherwise, every run through the game will be a Leshy run and only care about Leshy packs.

The metacategories are:

```
public enum PackInfo.PackMetacategory
{
    LeshyPack = 0,
    P03Pack = 1,
    GrimoraPack = 2,
    MagnificusPack = 3
}
```

## JSON Loader Cards
If you use [JSON Loader](https://inscryption.thunderstore.io/package/MADH95Mods/JSONCardLoader/) to build your cards, it's really easy to add a pack definition to your mod. Here's what you need to do:

1. Make sure all of your cards have the "modPrefix" attribute properly set. All cards from your mod should have the same mod prefix, which identifies them as belonging to the same card pack.
    - This alone is enough to make the pack manager function properly with your mod. It will autogenerate a pack for you, but it will not have a bespoke description or pack art. It will just be a blank pack icon that displays the card art of the card with the highest power level in your mod.
2. Include a pack file in your mod. This is a JSON file with the file extension *.jlpk that will contain the description of your mod pack, the title, and a custom pack art:

```json
{
	"Title": "Incredible Card Expansion",
	"Description": "This card pack is full of cards that will blow your mind.",
	"ModPrefix": "boom",
	"PackArt": "Artwork/boom_pack.png",
    "ValidFor": ["LeshyPack"]
}
```

## Using the API directly
You can also create a card pack using this API directly. All you need to do is ask the PackManager to create a PackInfo object for the mod prefix associated with your cards.

```c#
using Infiniscryption.PackManagement;

public static void CreatePack()
{
    PackInfo incrediPack = PackManager.GetPackInfo("boom");
    incrediPack.Title = "Incredible Card Expansion";
    incrediPack.SetTexture(TextureHelper.GetImageAsTexture("Artwork/boom_pack.png");
    incrediPack.Description = "This card pack is full of cards that will blow your mind.";
    incrediPack.ValidFor.Add(PackInfo.PackMetacategory.LeshyPack);
}
```

It is strongly recommended however that you do *not* create a hard dependency between your card pack and this API. Instead, you can create a soft dependency using the following pattern:

```c#
private void Start() // Do this in your Plugin.cs file
{
    if (Chainloader.PluginInfos.ContainsKey("zorro.inscryption.infiniscryption.packmanager"))
        CreatePack()
}
```

Using this pattern will allow you to create the pack when the user has installed the Pack Manager API, but will not force the user to have installed that API if they do not want it.

## Description placeholders

You can put the following placeholders into the description of your pack to help dynamically generate the text that will appear when the user hovers over it on the screen:

- \[count\]: Will be replaced with the number of cards in this pack.
- \[randomcard\]: Will be replaced with the name of a random card in the pack.
- \[name\]: Will be replaced with the title of this pack.
- \[powerlevel\]: Will be replaced with the average power level of cards in the pack.

## Pack validity

Each pack has a completely optional "ValidFor" property, which is a list of CardTemples. This is meant to allow you to indicate which zones/biomes the card pack is valid for. By default, the game only has a single zone available in Kaycee's Mod. That zone is Leshy's Cabin, which is the Nature zone. However, other mods, such as the "P03 for Kaycee's Mod" mod, may add other playable zones. The pack definition includes the idea of "ValidFor" in order to provide some amount of future-proofing.

## What if there's a special CardMetaCategory that I *really* need to not be removed by the Pack Manager?

There's an example of this: my side decks mod can't allow the SideDeck metacategory to be removed. To do this, you need to add a 'protected metacategory.' Here is the relevant snippet of code from my side decks mod. Notice how I wrapped the call to the pack manager in a separate method surrounded by a try-catch. This prevents a soft lock if the player hasn't installed the Pack Manager.

```c#
private static void RegisterMetacategoriesInner()
{
    PackManager.AddProtectedMetacategory(SideDeckManager.SIDE_DECK);
}

private void Start() // Do this in your Plugin.cs file
{
    if (Chainloader.PluginInfos.ContainsKey("zorro.inscryption.infiniscryption.packmanager"))
        RegisterMetacategoriesInner();
}
```

## Creating Pack Art

A template (blank) pack art PNG is included in this package.

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

1.0.8
- A personal message from DivisionByZ0rro

1.0.7
- Default encounters are no longer removed from the pool when the default card pack is turned off (configurable)
- Encounter switching can be toggled off with a configuration option
- Abilities are no longer removed from the Part 3 Modular or Part 3 Bounty Hunter pool if a card pack is removed

1.0.6
- Properly handle the case where the P03 In Kaycee's Mod plugin is uninstalled while the game is in P03 mode.

1.0.5
- The Rare metacategory was mistakenly being skipped when filtering the list of valid cards for each pack.

1.0.4
- Changed the internal JSON parser to resolve some defects.

1.0.3
- Found one more goof in the README and fixed it. 

1.0.2
- Fixed the README. I had a bad example for the JLPK and that wasted a of people's time. My bad.

1.0.1
- Like a dope, I managed to push a version of this mod that didn't have page scrolling activated. The mod can now handle more than 7 packs. Major facepalm.

1.0
- Initial version. 
</details>