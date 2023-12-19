# Original Card Renderer

**PLEASE NOTE THAT THIS MOD DOES NOTHING IF YOU DON'T ENABLE IT IN THE CONFIG FILE - READ THIS ENTIRE README PLEASE**

This mod forces cards to be rendered using the render mode of their native temple. That is - Undead cards are rendered as gravestones, Tech cards are rendered as disks, etc.

And that's *all* this does. This does *not* add card art for missing cards. It does not enable cards to display additional costs or extra sigils. I'm leaving all of that up to the API, the various Scrybe mods, and whatever else. The *only thing* this does is make cards render using their native forms.

The intent is for this mod to be an enabler for other mods. That's it.

(Okay - it also adds an attack animation for Magnificus cards in every zone other than the Magnificus zone. It does this because otherwise the Magnificus cards would literally do nothing when they attacked, which is...lame.)

This is an early version - and this is buggy! I've tested it a fair bit, but I'm still surprised by how oddly certain edge cases behave. I hope I've found them all, but I'm sure you'll find bugs. Please let me know and I'll try to squish them.

## How it works

Every card in the game has a property called `temple`, which is just an indicator that tells the game which Scrybe it belongs to. Act 1 only has `CardTemple.Nature`, Act 3 only has `CardTemple.Tech`, the Grimora section of the finale only has `CardTemple.Undead`, and the Magnificus section of the finale only has `CardTemple.Wizard`. Act 2 is the only act where cards from all temples interact.

However, the way the card appears is not connected to the temple. Instead, it is connected to the act. In Act 1, all cards look like paper cards. In Act 3, all cards look like disks. In Grimora's section, everything looks like a tombstone. Etc.

This mod changes this behavior. Now, in every zone (except Act 2!), cards will render according to their `temple`. If a card has `CardTemple.Tech`, it will render as a disk - no matter which act you are in.

### I'm confused - what good does this do if only Nature cards appear in Act 1?

That's why this is meant to be a helper mod for others; it's not really meant to be its own thing. 

One way that out-of-temple cards can appear in each act is through the use of the (Pack Management API)[https://inscryption.thunderstore.io/package/Infiniscryption/Pack_Management_API/], which allows modders to build packs of cards for Kaycee's Mod runs that can apply to multiple acts. You can put a bunch of `CardTemple.Tech` cards into a pack that's marked as valid for Leshy runs, and those cards will appear in Leshy runs whenever that pack is selected.

## How to use

**I'm installing this because it sounds like fun!** You need to install this, start the game, then edit the config file to set the "RendererAlwaysActive" flag to True. You then need a way for out-of-temple mods to appear in your game; the Pack Manger mod can help to some degree, but you need to set the `CrossOverAllPacks` config setting in that mod to `true` in order to get what you're looking for.

**I've made a custom card that I want to render in its native form.** You need to make this mod a dependency for your mod. Then, add an extension property with the ID `"Renderer.OverrideTemple"` to your card, where the value of that property is the temple you want the card to render as. 

```c#
// Make this card render like a Magnificus card
card.SetExtendedProperty("Renderer.OverrideTemple", CardTemple.Wizard);
```

**I'm writing some sort of other mod and I want to turn this feature on and off myself.** You can do this either permanently (i.e., it's always active), or temporarily (for example, if you only want it active during a certain event).

To permanently enable it, you set the `EnabledForAllCards` static property on the card renderer. Note that there is no protection on this; if you turn it on and another mod turns it off, the last one wins. This is purely here for convenience.

```c#
using Infiniscryption.DefaultRenderers;

// You can do this anywhere; inside of your plugin's Awake method is probably the best
DefaultCardRenderer.EnabledForAllCards = true;
```

To *temporarily* enable it ("temporarily" meaning "until the next Unity scene is loaded"), you need to get a reference to the current `Instance` of the card renderer and give it your name. As long as at least one mod says to turn it on, it will be on. You should *always* use null propagation on the `Instance` in case it isn't active for some reason:

```c#
using Infiniscryption.DefaultRenderers;

// Do this somewhere that's actively executing game code
// Don't do this in your plugin's Awake method; it won't do anything
DefaultCardRenderer.Instance?.EnableGlobalRenderer("my.plugin.guid");
```

Then, if you want to turn it back off, use this snippet. This will cancel your request to activate the renderer, but other mod requests could still be active.

```c#
using Infiniscryption.DefaultRenderers;

// Do this somewhere that's actively executing game code
// Don't do this in your plugin's Awake method; it won't do anything
DefaultCardRenderer.Instance?.CancelGlobalRenderer("my.plugin.guid");
```

You can also cleanly hook this into the Unity Scene Management framework:

```c#
using UnityEngine.SceneManagement;
using Infiniscryption.DefaultRenderers;

// Do this inside your plugin's Awake method
SceneManager.sceneLoaded += delegate(Scene scene, LoadSceneMode mode)
{
    DefaultCardRenderer.Instance?.EnableGlobalRenderer("my.plugin.guid");
};
```

## Changelog

<details>
<summary>Changelog</summary>

0.1.0
- Initial Version. Contains both features and bugs.

</details>