# Visually Stackable Sigils

This is a simple mod that replaces multiple icons for sigils that can stack with a single icon and number. With this mod installed, multiple instances of a stackable ability are represented with a single icon and a number, instead of multiple instances of that icon. This significantly cuts down on visual clutter on the card.

Previous versions of this mod also made a number of vanilla sigils stackable. This is no longer the case! In order to maintain compatibility with the community unofficial patch, 

## Kaycee's Mod is imminent...

Because Kaycee's Mod (the free expansion for Inscryption) is imminent, no more development will be done on any of my mods until that expansion hits. I will fix breaking bugs, however.

## How does this mod work?

This works by visually searching the ability icon, pixel by pixel, for the number 1. If the mod finds that exact pixel pattern, it will replace it with the correct value. If it cannot find that pixel pattern, it will look for an open space in the sigil where the number will fit. If that doesn't work, it will put the number in the lower-right corner, covering up whatever is already there.

Of course, the search for the number 1 is pretty fragile. It really is doing a pixel-by-pixel pattern match, so if there is a number in your sigil, but that number is even slightly off of what it expects, it won't replace it (which means you could end up with a sigil that has two numbers on it, which could be confusing).

If you want to make the icon for your sigil compatible with the automatic visual patching that this mod does, then you need to put one of the two images for '1' that come with this mod into your sigil. There are two files, 'Stack_1.png' (12x18 pixels) and 'Stack_1_med.png' (11x18 pixels) that hold the templates you need. The entire rectangular area that the number fills has to be *exactly* as it is in the original file, so if you choose 'Stack_1_med.png' as your default, there has to be an 11x18 space in your sigil that has exactly that 11x18 image of a '1' inside of it.

## Known Issues

Some of the game's original sigils don't always behave the way you would expect when stacked. Stacking abilities makes them trigger multiple times; it doesn't combine their effects into a single instance. So, for example, if a card has two Sharp on it, it will deal back 1 damage twice - it will not deal 2 damage once. So let's say that a card with Bees on Hit deals damage to a creature with two Sharps. The creature with two Sharps will hit the creature with Bees on Hit twice, and you will draw two bees. Value!

<details>
<summary>Changelog</summary>

1.0.4
- Updated dependencies and removed defect fix patch that is now fixed in api
- Fixed so that nonstackable sigils don't appear stackable when added by totems.
- Forked the vanilla stackable sigil portion of the mod into another mod.

1.0.3
- Repackaged DLL 

1.0.2
- Fixed defect in totem battle

1.0.0
- Initial version
</details>