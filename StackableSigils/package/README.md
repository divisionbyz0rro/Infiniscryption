# Visually Stackable Sigils

This is a simple mod that replaces multiple icons for sigils that can stack with a single icon and number. With this mod installed, multiple instances of a stackable ability are represented with a single icon and a number, instead of multiple instances of that icon. This significantly cuts down on visual clutter on the card.

This mod also makes a number of vanilla sigils stackable where they previously were not. The following sigils are now stackable:

- BeesOnHit
- BuffNeighbours
- BuffEnemy
- BuffGems
- ConduitBuffAttack
- DebuffEnemy
- DrawAnt
- DrawCopy
- DrawCopyOnDeath
- DrawRabbits
- DrawRandomCardOnDeath
- DrawVesselOnHit
- GainBattery
- Loot
- QuadrupleBones
- RandomConsumable
- Sentry
- Sharp
- Tutor

## How does this mod work?

This works by visually searching the ability icon, pixel by pixel, for the number 1. If the mod finds that exact pixel pattern, it will replace it with the correct value. If it cannot find that pixel pattern, it will look for an open space in the sigil where the number will fit. If that doesn't work, it will put the number in the lower-right corner, covering up whatever is already there.

Of course, the search for the number 1 is pretty fragile. It really is doing a pixel-by-pixel pattern match, so if there is a number in your sigil, but that number is even slightly off of what it expects, it won't replace it (which means you could end up with a sigil that has two numbers on it, which could be confusing).

If you want to make the icon for your sigil compatible with the automatic visual patching that this mod does, then you need to put one of the two images for '1' that come with this mod into your sigil. There are two files, 'Stack_1.png' (12x18 pixels) and 'Stack_1_med.png' (11x18 pixels) that hold the templates you need. The entire rectangular area that the number fills has to be *exactly* as it is in the original file, so if you choose 'Stack_1_med.png' as your default, there has to be an 11x18 space in your sigil that has exactly that 11x18 image of a '1' inside of it.

<details>
<summary>Changelog</summary>

1.0.2
- Fixed defect in totem battle

1.0.0
- Initial version
</details>