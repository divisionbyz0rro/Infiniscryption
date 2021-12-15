# Side Deck Selector

Are you tired of those boring old squirrels? Have I got the thing for you...

This mod comes with six different starter decks to select from:
- **Squirrels:** Now with the ability to block fliers!
- **Bees:** You don't even need to find the bee totem anymore - but they do have Brittle now.
- **Ants:** Want some help making those ants a little better? These 1/Ant helpers will buff your little ant army for free.
- **Squids:** These squishy and slippery things have no bones, but are really hard to pin down.
- **Puppies:** Adorable. Energetic. They move from place to place and dig up a fresh bone for you every turn.
- **Goats:** They generate twice the blood when they die, but they knock out two of your teeth for the trouble.

There's also a hook to allow you or others to add more sidedeck cards to the pool; see below.

## Kaycee's Mod is imminent...

Because Kaycee's Mod (the free expansion for Inscryption) is imminent, no more development will be done on any of my mods until that expansion hits. I will fix breaking bugs, however.

## Requirements

As with most mods, you need [BepInEx](https://inscryption.thunderstore.io/package/BepInEx/BepInExPack_Inscryption/) installed. 

You will also need the [API](https://inscryption.thunderstore.io/package/API_dev/API/) installed, and the super helpful [Health for Ants](https://inscryption.thunderstore.io/package/JulianMods/HealthForAnts/) mod as well.

## Installation

The zip file should be structured in the same way as your Inscryption root directory. Just drop the 'BepInEx' folder into your Inscryption directory and you're golden.

## I want to develop a new starter deck card and use it here. How do I do that?

You need to add a specific trait to your card. There of course isn't a trait for 'sidedeck' card, so we have to make one up. C# is weird; traits are enumerated, but you're allowed to cast any integer to an enum and it won't complain. In this case, the specific trait number you need is '5103'.

Why 5103? SIDE. 5103. Do you see it? They look...somewhat similar. And hopefully now that you've seen it, you'll remember it in the future.

Just add '(Trait)5103' to your card, and it will get picked up by this mod and be a valid sidedeck card.

Example code:

```
NewCard.Add(
    "Sample_Card_ID",
    "Dummy Card",
    0, 1,
    new List<CardMetaCategory>() { },
    CardComplexity.Vanilla,
    CardTemple.Nature,
    "This is an example of a sidedeck card",
    defaultTex: myTexture,
    traits: new List<Trait>() { (Trait)5103 }
);
```

**Note:** If there are more than 12 possible side deckcards in the pool, 12 will be selected at random for you to choose from when you encounter the sidedeck node at the start of the game.

## Someone added a sidedeck card in a modpack I downloaded and I don't want that. What do it do?
Open the BepInEx config file for this mod (zorro.inscryption.infiniscryption.sidedecks.cfg) and set 'PullFromCardPool' to false. It will then only give you the cards added specifically in this mod.

## Changelog 

<details>
<summary>Changelog</summary>

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