# Infiniscryption
A rogue-lite mod for Inscryption

# What is Infiniscryption?
This mod aims to transform the first act of Inscryption into a run-based experience, with all of the bells and whistles you would come to expect. I'm specifically going to be referencing Hades a lot in terms of design decisions and features.

My goal is to, wherever possible, make the game compatible with other mods, *especially* those that add additional cards.

## Stability
Right now, the mod is very much in a pre-alpha stage. I will try to keep this as up to date as possible with the state of all of the features.

# Game Features
This section is a sort of mini-design doc for the mod.

## Starter Decks (Working)
The starting deck for the player character is now different. Rather than being given the same starter deck every time, the player will be dealt an empty deck and then be given the option to select their starting deck in the very first event on the map.

## Reduce Power of Consumables (Planned)
This is one of the few difficulty tweaks I'm going to place into the base game. Look, consumables are too powerful, and you can just use them as freely as you want because they completely refill at every backpack event. I'm just going to make a simple tweak here - backpacks can never give you more than one item.

## Force Reset (Planned)
This is pretty simple. Once you beat Leshy, you need to be forced to start over. Think of it like beating Hades. Even once you beat the final boss of that game, you keep playing. So basically, this means I need to break the film roll event and prevent you from ever collecting the film roll. Once that happens, you'll keep playing the game over and over again.

## Increasing Difficulty (Planned)
The goal is to make something akin to the Hades "Pact of Punishment," where you opt-in to harder difficulty modifiers as the game progresses in order to keep the challenge flowing.

This is really important, because the base game is very cleverly designed to make it so that you're almost overpowered if you keep going and solving cabin puzzles. A lot of the fun of making the game keep going would be lost if the difficulty didn't scale up.

Each of the potential modifiers has a varying degree of implementation difficulty. This is probably going to be the most challenging of all of the plugins to develop because of how many things it needs to touch. I also have to come up with some sort of UI to let the user select their modifiers, which might actually end up being the most difficult part of the whole thing.

Difficulty modifiers I'm considering:

- **Reduced Consumables**: Reduce the max nubmer of consumables
- **Harder 'normal' Fights**: Increases the run's difficulty modifier
- **Harder Bosses**: Ideally, I would like to add phases to bosses, but that's...going to be a lot of work!
- **Handicap**: Start each round -1 tooth.
- **Give Up Upgrades**: Turn off things like the clover, the bees, etc.

## Meta-Currency (In Progress)
As with every rogue-like, there has to be some sort of meta-currency that can be spent for permanent upgrades. Currently, I'm hoping to implement two meta-currencies:

- **Leftover Teeth:** Store any leftover teeth at the end of the run. I'm aware that the Oroboros breaks this to a degree. I'll worry about that later.
- **Quills:** Use the quills that Leshy offers whenever he concedes as a second meta-currency.

I'm also considering adding additional currencies tied to boss battles, very much the way that Hades works (as in, something you collect for beating a boss, but only the first time you beat it at a given difficulty level).

But of course, what do you spend these on?

- **Additional Starter Decks:** Unlock additional starter decks beyond the three currently implemented.
- **Upgrade Starter Decks:** Replace cards in each deck with more powerful versions.
- **Systemic Upgrades:** The third candle, the lucky clover, etc.

You'll observe that a lot of these things I'm talking about directly relate to things that would ordinarily have been solved with puzzles in the base game. My intention is to put a gatekeeper over collecting key items in the game that require you to own a set amount of meta-currency. For example, prevent you from picking up the third candle unless you can spend 10 quills (or something like that).

I'm still undecided on whether or not to keep the existing puzzles in place. It might be annoying to have to both solve the puzzle *and* have the required currency to upgrade. It's trivial to have the puzzles solve themselves at the start of the game.

# Project Architecture
The mod is implemented as a series of plugins. Each of these plugins has an 'Active' flag that will allow you to activate or deactivate individual components of this mod, in case you only want to use certain features. If the mod is deactivated, it will skip patching the game when it starts up.

## InfiniscryptionStarterDeckPlugin
This plugin implements the 'Starter Decks' feature.

### Configuration

- **Active** (Boolean): Turns the plugin on or off.
- **Deck\[1/2/3]** (String): A comma-separated string defining each of the three starter decks. This value is used to populate the starter decks in the save file when the game is first loaded or reset through chapter select. The first card in the deck is referred to as the 'leader;' this is the card the player will see in the card selection event at the start of the game (so if you change this, make sure the first card of each deck is able to easily uniquely identify a given deck).

### Behavior and Implementation
This plugin works by doing the following:

1. Preventing the execution of code that creates the player's starting deck (forcing them to start with an empty deck).
2. Replacing the first event of the map with a Tribal Selection event.
3. Overriding the card selection for that event.
4. Adding additional cards to the player's deck if they ever end a card selection event with only a single card in their deck.
5. Modifying the run start sequence to skip the deck review section (since there is nothing to review).
6. Modifying the dialogue in card selection events when the player has no cards in their deck.

### Deck Upgrades
This plugin currently does not supporting upgrading starter decks. A separate plugin will handle the collection of meta-currency; at that point I will figure out how those plugins will interact to allow you to upgrade your starter decks.