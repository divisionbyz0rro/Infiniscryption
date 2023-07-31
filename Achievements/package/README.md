# Achievements for Kaycee's Mod (Beta)

This is an Achievement Management API. On its own, it doesn't do much other than allow you to browse the 8 Kaycee's Mod achievements and get a pop-up notification whenever you unlock one of them. The main draw of this API is that other mods can use this to add their own achievements which players can track in a dedicated achievement tracking screen.

The upshot is that you probably won't ever need to install this on by itself. Instead, other mods that want to add achievements will make this one of their dependencies.

This is in beta! Bugs are to be expected. @ me on discord if you need to.

## Browsing Achievements

Achievements can be viewed in the Kaycee's Mod section of the game, in a new screen under "UNLOCKS". Browse to UNLOCKS >> ACHIEVEMENTS to browse all currently available achievments.

NOTE: To protect the immersive nature of the default Inscryption experience, only base game achievements specifically for Kaycee's Mod have been made compatible with this API. Achievements related to the main story of Inscryption will not be "unlocked" using the achievement popup and will not be browseable in the Achievements screen.

## Achievement Groups

All achievements must belong to an achievement group. Achievements will be displayed as part of their group on the achievement browser screen. All achievements in the same group will share the same "locked" icon and unlock sound. If you don't provide an unlock sound, this API will use a default (see the Credits section for a link to the sound); however, you *must* provide an unlock icon. All achievement icons must be a 22x22 `Texture2D` instance.

The unlock sound can either be an instance of `AudioClip` (most easily loaded through the API's `SoundManager` class), or it can be the *name* of a preloaded audio clip. The game loads all clips in the `Resources/Audio/SFX` folder at launch, so if you've unpacked the game's file you can browse through them and look for an existing audio clip to use - just supply the name of the `wav` file.

You will need to keep a reference to the ID of the achievement group that was generated for you when you call the `NewGroup` method. This ID will be used to link the achievements you create to that group. 

```c#
using Infiniscryption.Achievements;
using InscryptionAPI.Helpers;
using InscryptionAPI.Sound;

// Creating a group with an unlock sound loaded from a wav file
var groupId = ModdedAchievementManager.NewGroup(
    "my.plugin.guid",       // plugin guid
    "Example Achievements",  // achievement group name
    SoundManager.LoadAudioClip("achievement_default.wav"), // unlock sound
    TextureHelper.GetImageAsTexture("achievement_locked.png", typeof(MyPlugin).Assembly) // lock icon
).ID;

// Creating a group with an unlock sound that re-uses an existing game asset
var groupId = ModdedAchievementManager.NewGroup(
    "my.plugin.guid",       // plugin guid
    "Example Achievements", // achievement group name
    "VO_yes",               // unlock sound - Luke Carder shouting "YES!"
    TextureHelper.GetImageAsTexture("achievement_locked.png", typeof(MyPlugin).Assembly) // lock icon
).ID;

// Creating a group without an unlock sound (which uses the default)
var groupId = ModdedAchievementManager.NewGroup(
    "my.plugin.guid",       // plugin guid
    "Example Achievements",  // achievement group name
    TextureHelper.GetImageAsTexture("achievement_locked.png", typeof(MyPlugin).Assembly) // lock icon
).ID;
```

## Achievements

Once you have created an achievement group, you can start creating new achievements. Each achievement needs a name, a description, an icon, and an indicator as to whether or not it is a "secret" (secret achievements will not display their description on the achievement screen until they have been unlocked).

As with the locked icon, the achievement icon must be a 22x22 `Texture2D` instance.

You will need to keep a reference to the achievement IDs that were created for you so you can write code to unlock them later:

```c#
using Infiniscryption.Achievements;
using InscryptionAPI.Helpers;

internal static Achievement SAMPLE_ACHIEVEMENT { get; private set; }

SAMPLE_ACHIEVEMENT = ModdedAchievementManager.New(
    "my.plugin.guid",       // plugin guid
    "This Is Only A Test",  // Achievement name
    "This achievement exists solely to show how the API works", // Achievement description
    false,                  // Indicates if the achievement is secret
    groupId,                // The ID of the achievement group from above
    TextureHelper.GetImageAsTexture("achievement_test.png", typeof(MyPlugin).Assembly)
).ID;
```

## Unlocking Achievements

All achievements are unlocked the same way: using the base game method `AchievementManager.Unlock`.

HINT: One very common place to unlock achievements is at the end of a Kaycee's Mod run. The best way to do this is to patch the `AscensionMenuScreens.TryUnlockAchievements` method:

```c#
[HarmonyPatch(typeof(AscensionMenuScreens), nameof(AscensionMenuScreens.TryUnlockAchievements))]
[HarmonyPostfix]
private static void AscensionCompleteAchievements()
{
    if (/* condition */)
        AchievementManager.Unlock(SAMPLE_ACHIEVEMENT);
}
```

## Checking To See If An Achievement Is Unlocked

If you want to track a specific achievement's status (for example, if you want to manage some sort of special reward), you can check like this:

```c#
using Infiniscryption.Achievements;

ModdedAchievementManager.AchievementById(SAMPLE_ACHIEVEMENT).IsUnlocked; // Tells you if the achievement is unlocked
```

## Credits

Achievement sound: ["UI App > UI Achievement Puzzle Game Application"](https://freesound.org/people/Eponn/sounds/636660/) by [Eponn](https://freesound.org/people/Eponn/)

The Thunderstore icon for this mod (which appears nowhere else in the mod) came from [IconMonstr](https://iconmonstr.com/trophy-6-svg/)

## Changelog

<details>
<summary>Changelog</summary>

0.1.1
- Fixed typo in the README.
- Removed unhelpful errors getting written to the log file

0.1
- Beta version of Achievements

</details>