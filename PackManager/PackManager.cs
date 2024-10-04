using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using BepInEx.Bootstrap;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.PackManagement.Patchers;
using InscryptionAPI.Card;
using InscryptionAPI.Encounters;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using InscryptionAPI.Regions;
using InscryptionAPI.Saves;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Infiniscryption.PackManagement
{
    [HarmonyPatch]
    public static class PackManager
    {
        private static List<CardMetaCategory> protectedMetacategories = new();
        private static List<AbilityMetaCategory> protectedAbilityMetacategories = new()
        {
            AbilityMetaCategory.Part1Rulebook,
            AbilityMetaCategory.Part3Rulebook,
            AbilityMetaCategory.GrimoraRulebook,
            AbilityMetaCategory.MagnificusRulebook,
            AbilityMetaCategory.BountyHunter
        };

        // Mod compatibility
        private const string GRIMORA_MOD = "arackulele.inscryption.grimoramod";
        private const string P03_MOD = "zorro.inscryption.infiniscryption.p03kayceerun";
        private const string MAGNIFICUS_MOD = "silenceman.inscryption.magnificusmod";

        private static readonly CardMetaCategory GRIMORA_CHOICE_NODE = GuidManager.GetEnumValue<CardMetaCategory>(GRIMORA_MOD, "GrimoraModChoiceNode");

        /// <summary>
        /// Adds a protected metacategory to the internal protected list.
        /// </summary>
        /// <remarks>Protected metacategories are never removed from cards, even if the packs those cards are in 
        /// are excluded by the player.</remarks>
        public static void AddProtectedMetacategory(CardMetaCategory category) => protectedMetacategories.Add(category);

        /// <summary>
        /// Adds a protected metacategory to the internal protected list.
        /// </summary>
        /// <remarks>Protected metacategories are never removed from abilities, even if there are no cards with the 
        /// abilities valid in the card pool.</remarks>
        public static void AddProtectedMetacategory(AbilityMetaCategory category) => protectedAbilityMetacategories.Add(category);

        static PackManager()
        {
            // Add all of the default packs
            PackInfo beastly = new PackInfo(CardTemple.Nature);
            beastly.Title = "Inscryption: Beastly Card Expansion Pack";
            beastly.Description = "The original set of cards featured in Leshy's cabin. Featuring wolves, mantises, and the occasional cockroach.";
            beastly.SetTexture(TextureHelper.GetImageAsTexture("beastly.png", typeof(PackManager).Assembly));
            AddPack(beastly);

            PackInfo techno = new PackInfo(CardTemple.Tech);
            techno.Title = "Inscryption: Techno Card Expansion Pack";
            techno.Description = "The original set of robotic cards, exclusively using the energy mechanic.";
            techno.SetTexture(TextureHelper.GetImageAsTexture("tech.png", typeof(PackManager).Assembly));
            AddPack(techno);

            PackInfo undead = new PackInfo(CardTemple.Undead);
            undead.Title = "Inscryption: Undead Card Expansion Pack";
            undead.Description = "Powered by the bones of the dead, these cards have come back from the grave to fight for you.";
            undead.SetTexture(TextureHelper.GetImageAsTexture("undead.png", typeof(PackManager).Assembly));
            AddPack(undead);

            PackInfo wizard = new PackInfo(CardTemple.Wizard);
            wizard.Title = "Inscryption: Magickal Card Expansion Pack";
            wizard.Description = "Harness the might of the moxen to summon forth apprentices and fight in the most honorable of duels.";
            wizard.SetTexture(TextureHelper.GetImageAsTexture("wizard.png", typeof(PackManager).Assembly));
            AddPack(wizard);

            // Add the leftovers pack as well
            PackInfo leftovers = new PackInfo(true);
            leftovers.Title = "Miscellaneous Community Cards";
            leftovers.Description = "The unusual, unsorted, and unruly cards that have been added by mods but not properly sorted into packs.";
            leftovers.SetTexture(TextureHelper.GetImageAsTexture("leftovers.png", typeof(PackManager).Assembly));
            AddPack(leftovers);

            // Leshy encounters pack
            EncounterPackInfo beastEncounters = new EncounterPackInfo(CardTemple.Nature);
            beastEncounters.Title = "Inscryption: Beastly Encounters";
            beastEncounters.Description = "The original set of [summary] that Leshy has carefully curated for the ideal gaming experience.";
            AddPack(beastEncounters);

            EncounterPackInfo leftoverEncounters = new EncounterPackInfo(true);
            leftoverEncounters.Title = "Unsorted Regions and Encounters";
            leftoverEncounters.Description = "These [summary] have been added by mods but not properly sorted into packs.";
            leftoverEncounters.SetTexture(TextureHelper.GetImageAsTexture("leftovers_encounters.png", typeof(PackManager).Assembly));
            AddPack(leftoverEncounters);

            AddPack(new EncounterPackInfo(CardTemple.Tech) { ValidFor = new() { PackInfo.PackMetacategory.P03Pack } });

            // Temple metacategories
            TempleMetacategories = new Dictionary<CardTemple, List<CardMetaCategory>>();
            TempleMetacategories.Add(CardTemple.Nature, new() { CardMetaCategory.ChoiceNode, CardMetaCategory.TraderOffer, CardMetaCategory.Rare });
            TempleMetacategories.Add(CardTemple.Tech, new() { CardMetaCategory.ChoiceNode, CardMetaCategory.Part3Random, CardMetaCategory.Rare });
            TempleMetacategories.Add(CardTemple.Wizard, new() { CardMetaCategory.ChoiceNode, CardMetaCategory.Rare });
            TempleMetacategories.Add(CardTemple.Undead, new() { CardMetaCategory.ChoiceNode, CardMetaCategory.Rare, GRIMORA_CHOICE_NODE });
        }

        /// <summary>
        /// The list of categories that make a card valid in a particular temple.
        /// </summary>
        /// <remarks>The point of this is to deal with the fact that simply belonging to a particular temple
        /// doesn't make a card playable in that region. For example, having CardTemple.Nature doesn't mean a card 
        /// is playable in a Leshy run; you also need either ChoiceNode (so you can get it from a three-card selection event)
        /// or TraderOffer (so it will appear tradable for pelts) or Rare (so you can get it after a boss). If you don't
        /// have one of these three metacategories, the card isn't playable, and so the Pack Manager will ignore it.
        /// 
        /// The purpose of making this public is to allow other modders to affect this behavior. For example, the P03 mod
        /// has five new custom metacategories for its special choice nodes, so it adds those here to the Tech temple.</remarks>
        public static Dictionary<CardTemple, List<CardMetaCategory>> TempleMetacategories;

        // internal static List<PackInfo> AllPacks;

        // internal static List<EncounterPackInfo> AllEncounterPacks;

        internal static Dictionary<string, List<PackInfoBase>> AllPacksOfType = new();

        private static HashSet<string> ActiveCards = new();

        private static HashSet<Ability> ActiveAbilities = new();

        /// <summary>
        /// Sorts an enumeration of packs in the order they should appear on the pack management screen
        /// </summary>
        public static IEnumerable<T> InScreenOrder<T>(this IEnumerable<T> packs) where T : PackInfoBase
        {
            return packs.Where(pi => pi.IsBaseGameCardPack)
                   .Concat(packs.Where(pi => pi.IsStandardCardPack))
                   .Concat(packs.Where(pi => pi.IsLeftoversPack));
        }

        internal static IEnumerable<PackInfoBase> AllPacks(Type type)
        {
            string key = type.Name;
            if (!AllPacksOfType.ContainsKey(key))
                AllPacksOfType.Add(key, new());
            return AllPacksOfType[key];
        }

        internal static IEnumerable<T> AllPacks<T>() where T : PackInfoBase
        {
            return AllPacks(typeof(T)).Cast<T>();
        }

        internal static void AddPackByType(Type type, PackInfoBase pack)
        {
            string key = type.Name;
            if (!AllPacksOfType.ContainsKey(key))
                AllPacksOfType.Add(key, new());
            AllPacksOfType[key].Add(pack);
        }

        internal static void AddPack<T>(T pack) where T : PackInfoBase
        {
            AddPackByType(typeof(T), pack);
        }

        /// <summary>
        /// List of all registered packs.
        /// </summary>
        public static IEnumerable<PackInfo> AllRegisteredPacks => AllPacks<PackInfo>().InScreenOrder();

        /// <summary>
        /// Gets the pack definition for one of the four default packs (the packs that represent the cards that shipped in the
        /// base game).
        /// </summary>
        public static PackInfo GetDefaultPackInfo(CardTemple temple) => AllPacks<PackInfo>().FirstOrDefault(pi => pi.DefaultPackTemple == temple);

        /// <summary>
        /// Gets the pack definition for a pack that represents content from the base game.
        /// base game).
        /// </summary>
        public static T GetDefaultPackInfo<T>(CardTemple temple) where T : PackInfoBase => AllPacks<T>().FirstOrDefault(pi => pi.DefaultPackTemple == temple);

        /// <summary>
        /// Gets the pack containing cards for a given mod prefix.
        /// </summary>
        [Obsolete]
        public static PackInfo GetPackInfo(string modPrefix)
        {
            PackInfo retval = AllPacks<PackInfo>().FirstOrDefault(pi => pi.IsStandardCardPack && modPrefix.Equals(pi.ModPrefix));
            if (retval == null)
            {
                // We're being asked to generate a default pack based on a mod prefix
                retval = new PackInfo(modPrefix, PackInfo.PackMetacategory.LeshyPack);
                AddPack(retval);
            }

            return retval;
        }

        internal static PackInfoBase GetPackInfo(Type type, string modPrefix)
        {
            PackInfoBase retval = AllPacks(type).FirstOrDefault(pi => pi.IsStandardCardPack && modPrefix.Equals(pi.ModPrefix));
            if (retval == null)
            {
                // We're being asked to generate a default pack based on a mod prefix
                retval = (PackInfoBase)Activator.CreateInstance(type, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public, null, new object[] { modPrefix }, CultureInfo.InvariantCulture);
                AddPack(retval);
            }

            return retval;
        }

        /// <summary>
        /// Gets the pack containing items for a given mod prefix.
        /// </summary>
        public static T GetPackInfo<T>(string modPrefix) where T : PackInfoBase
        {
            T retval = AllPacks<T>().FirstOrDefault(pi => pi.IsStandardCardPack && modPrefix.Equals(pi.ModPrefix));
            if (retval == null)
            {
                // We're being asked to generate a default pack based on a mod prefix
                retval = (T)Activator.CreateInstance(typeof(T), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public, null, new object[] { modPrefix }, CultureInfo.InvariantCulture);
                AddPack(retval);
            }

            return retval;
        }

        internal static T GetEstablishedPackInfo<T>(string modPrefix, PackInfo.PackMetacategory category) where T : PackInfoBase
        {
            T retval = AllPacks<T>().FirstOrDefault(pi => !string.IsNullOrEmpty(pi.ModPrefix) && pi.ModPrefix.Equals(modPrefix, StringComparison.InvariantCultureIgnoreCase));
            if (retval == null)
            {
                // We're being asked to generate a default pack based on a mod prefix
                retval = (T)Activator.CreateInstance(typeof(T), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public, null, new object[] { modPrefix }, CultureInfo.InvariantCulture);
                retval.ValidFor = new() { category };
                AddPack(retval);
            }

            return retval;
        }

        internal static PackInfo GetEstablishedPackInfo(string modPrefix)
        {
            PackInfo retval = AllPacks<PackInfo>().FirstOrDefault(pi => pi.IsStandardCardPack && modPrefix.Equals(pi.ModPrefix));
            if (retval == null)
            {
                // We're being asked to generate a default pack based on a mod prefix
                retval = new PackInfo(modPrefix, PackInfo.PackMetacategory.LeshyPack);
                retval.ValidFor.Add(PackInfo.PackMetacategory.P03Pack);
                retval.ValidFor.Add(PackInfo.PackMetacategory.MagnificusPack);
                retval.ValidFor.Add(PackInfo.PackMetacategory.GrimoraPack);
                retval.SplitPackByCardTemple = true;
                AddPack(retval);
            }

            return retval;
        }

        /// <summary>
        /// Gets the pack definition for the "leftovers" pack (that is, all random cards that haven't been put into a pack
        /// properly)
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public static PackInfo GetLeftoversPack()
        {
            return AllPacks<PackInfo>().FirstOrDefault(pi => pi.IsLeftoversPack);
        }

        /// <summary>
        /// Gets the pack definition for the "leftovers" pack (that is, all random cards that haven't been put into a pack
        /// properly)
        /// </summary>
        /// <returns></returns>
        public static T GetLeftoversPack<T>() where T : PackInfoBase
        {
            return AllPacks<T>().FirstOrDefault(pi => pi.IsLeftoversPack);
        }

        internal static Dictionary<string, string> AcceptedScreenStates = new()
        {
            { P03_MOD, P03_MOD },
            { GRIMORA_MOD, GRIMORA_MOD },
            { MAGNIFICUS_MOD, $"{MAGNIFICUS_MOD}starterdecks" }
        };

        internal static CardTemple ScreenState
        {
            get
            {
                Scene activeScene = SceneManager.GetActiveScene();
                if (activeScene != null && !string.IsNullOrEmpty(activeScene.name))
                {
                    string sceneName = activeScene.name.ToLowerInvariant();
                    if (sceneName.Contains("magnificus"))
                        return CardTemple.Wizard;
                    if (sceneName.Contains("part3"))
                        return CardTemple.Tech;
                    if (sceneName.Contains("grimora"))
                        return CardTemple.Undead;
                    if (sceneName.Contains("part1"))
                        return CardTemple.Nature;
                }

                foreach (string guid in AcceptedScreenStates.Keys)
                {
                    if (!Chainloader.PluginInfos.ContainsKey(guid))
                        continue;

                    string value = ModdedSaveManager.SaveData.GetValue(AcceptedScreenStates[guid], "ScreenState");
                    if (string.IsNullOrEmpty(value))
                        continue;

                    return (CardTemple)Enum.Parse(typeof(CardTemple), value);
                }

                return CardTemple.Nature;
            }
        }

        internal static void ForceSyncOfAllPacks()
        {
            foreach (var grp in CardManager.AllCardsCopy.Where(ci => !ci.IsBaseGameCard()).GroupBy(ci => ci.GetModPrefix()))
            {
                if (string.IsNullOrEmpty(grp.Key))
                    continue;

                if (grp.Count() > 5)
                    PackManager.GetEstablishedPackInfo(grp.Key);
            }

            PackPlugin.Log.LogInfo($"Looking for encounters");
            foreach (var ebd in EncounterManager.NewEncounters.Where(e => PackManager.AllPacks<EncounterPackInfo>().Where(epi => !epi.IsLeftoversPack).FirstOrDefault(epi => epi.PackContents.Contains(e.name)) == null).ToList())
            {
                PackPlugin.Log.LogInfo($"Found unpacked encounter {ebd.name}");
                var cardPrefix = EncounterPackInfo.GetModPrefixesFromEncounter(ebd);
                if (!string.IsNullOrEmpty(cardPrefix.Item2))
                {
                    EncounterPackInfo pInfo = PackManager.GetEstablishedPackInfo<EncounterPackInfo>(cardPrefix.Item2, PackInfo.GetTempleDefaultMetacategory(cardPrefix.Item1));
                    PackPlugin.Log.LogInfo($"Established pack {pInfo.Title}");
                }
            }
        }

        internal static bool CardIsValidForScreenState(this CardInfo info)
        {
            try
            {
                int x = info.PowerLevel;

                foreach (CardMetaCategory cat in TempleMetacategories[ScreenState])
                    if (info.metaCategories.Contains(cat))
                        return true;

                return false;
            }
            catch (Exception ex)
            {
                PackPlugin.Log.LogError($"Error checking {info.name}");
                PackPlugin.Log.LogError(ex);
                return false;
            }
        }

        internal static void SavePackList(Type type, IEnumerable<PackInfoBase> packs, bool activeList, CardTemple state = CardTemple.Nature)
        {
            if (activeList)
            {
                SavePackList(type, InvertPackList(type, packs.ToList(), state), false, state);
                return;
            }

            string prefix = type == typeof(PackInfo) ? "" : type.Name;

            if (state == CardTemple.Nature)
                state = ScreenState;

            string packString = string.Join("|", packs.Select(pi => pi.Key));
            string packKey = activeList ? $"{prefix}{state.ToString()}_ActivePackList" : $"{prefix}{state.ToString()}_InactivePackList";

            ModdedSaveManager.SaveData.SetValue(PackPlugin.PluginGuid, packKey, packString);
        }

        internal static void SavePackList<T>(List<T> packs, bool activeList, CardTemple state = CardTemple.Nature) where T : PackInfoBase
        {
            SavePackList(typeof(T), packs.Cast<PackInfoBase>(), activeList, state);
        }

        internal static List<PackInfoBase> InvertPackList(Type type, List<PackInfoBase> packList, CardTemple state)
        {
            // The inversion of a list is "all packs which are not in this list"
            if (state == CardTemple.Nature)
                state = ScreenState;

            var valid = PackInfo.GetTempleDefaultMetacategory(state);
            return AllPacks(type).Where(p => p.ValidFor.Contains(valid)).Where(p1 => !packList.Any(p2 => p1.Key.Equals(p2.Key))).ToList();
        }

        internal static List<PackInfoBase> RetrievePackList(Type type, bool activeList, CardTemple state = CardTemple.Nature)
        {
            if (activeList)
                return InvertPackList(type, RetrievePackList(type, false, state), state);

            string prefix = type == typeof(PackInfo) ? "" : type.Name;

            if (state == CardTemple.Nature)
                state = ScreenState;

            string packKey = activeList ? $"{prefix}{state.ToString()}_ActivePackList" : $"{prefix}{state.ToString()}_InactivePackList";
            string packString = ModdedSaveManager.SaveData.GetValue(PackPlugin.PluginGuid, packKey);

            if (packString == default(string))
            {
                if (activeList)
                    return AllPacks(type).Where(pi => pi.ValidFor.Contains(PackInfo.GetTempleDefaultMetacategory(state))).ToList();
                else
                    return new();
            }

            return packString.Split('|').Select(k => AllPacks(type).FirstOrDefault(pi => pi.Key == k)).Where(pi => pi != null).ToList();
        }

        internal static List<T> RetrievePackList<T>(bool activeList, CardTemple state = CardTemple.Nature) where T : PackInfoBase
        {
            return RetrievePackList(typeof(T), activeList, state).Cast<T>().ToList();
        }

        /// <summary>
        /// Gets all active packs
        /// </summary>
        [Obsolete]
        public static List<PackInfo> GetActivePacks()
        {
            return RetrievePackList<PackInfo>(true);
        }

        public static List<T> GetActivePacks<T>() where T : PackInfoBase
        {
            return RetrievePackList<T>(true);
        }

        internal static List<CardInfo> FilterCardsInPacks(List<CardInfo> cards)
        {
            if (!ShouldFilterCards)
                return cards;

            List<PackInfo> activePacks = RetrievePackList<PackInfo>(true);

            string activePackDebugString = String.Join(", ", activePacks.Select(ap => ap.Title));
            PackPlugin.Log.LogDebug($"Active packs are {activePackDebugString}");

            ActiveCards = new();
            ActiveAbilities = new();

            foreach (PackInfo pack in activePacks)
            {
                List<CardInfo> cardsInPack = pack.Cards.ToList();
                PackPlugin.Log.LogDebug($"Evaluating pack {pack.Title} with {cardsInPack.Count}; split by temple {pack.SplitPackByCardTemple}. Temple is {ScreenState}");
                if (pack.SplitPackByCardTemple)
                    cardsInPack = cardsInPack.Where(ci => ci.temple == ScreenState).ToList();
                foreach (string cardName in cardsInPack.Select(ci => ci.name))
                    ActiveCards.Add(cardName);
            }

            foreach (CardInfo card in cards)
            {
                if (ActiveCards.Contains(card.name))
                {
                    PackPlugin.Log.LogDebug($"{card.name} is in an active pack; setting temple to {ScreenState}");
                    card.SetExtendedProperty("PackManager.OriginalTemple", card.temple);
                    card.temple = ScreenState;
                }
                else
                {
                    PackPlugin.Log.LogDebug($"{card.name} is NOT in an active pack; clearing metacategories");
                    card.metaCategories = new(card.metaCategories.Where(c => protectedMetacategories.Contains(c)));
                }

                if (ActiveCards.Contains(card.name))
                {

                    foreach (Ability ab in card.Abilities)
                        if (!ActiveAbilities.Contains(ab))
                            ActiveAbilities.Add(ab);
                    if (card.iceCubeParams != null && card.iceCubeParams.creatureWithin != null)
                        foreach (Ability ab in card.iceCubeParams.creatureWithin.Abilities)
                            if (!ActiveAbilities.Contains(ab))
                                ActiveAbilities.Add(ab);
                    if (card.evolveParams != null && card.evolveParams.evolution != null)
                        foreach (Ability ab in card.evolveParams.evolution.Abilities)
                            if (!ActiveAbilities.Contains(ab))
                                ActiveAbilities.Add(ab);
                }
            }

            PackPlugin.Log.LogInfo($"The final card pool has {ActiveCards.Count} cards and {ActiveAbilities.Count} abilities");

            return cards;
        }

        private static AbilityMetaCategory GetRulebookMetacategory(this CardTemple temple)
        {
            if (temple == CardTemple.Nature)
                return AbilityMetaCategory.Part1Rulebook;
            if (temple == CardTemple.Tech)
                return AbilityMetaCategory.Part3Rulebook;
            if (temple == CardTemple.Wizard)
                return AbilityMetaCategory.MagnificusRulebook;
            if (temple == CardTemple.Undead)
                return AbilityMetaCategory.GrimoraRulebook;

            return AbilityMetaCategory.Part1Rulebook;
        }

        internal static List<AbilityManager.FullAbility> FilterAbilitiesInPacks(List<AbilityManager.FullAbility> abilities)
        {
            if (!ShouldFilterCards)
                return abilities;

            // If the ability doesn't appear on any of the cards, 
            // it shouldn't appear anywhere
            foreach (var fab in abilities)
            {
                if (!ActiveAbilities.Contains(fab.Id))
                    fab.Info.metaCategories = new(fab.Info.metaCategories.Where(c => protectedAbilityMetacategories.Contains(c)));
                else
                {
                    fab.Info.metaCategories = new List<AbilityMetaCategory>(fab.Info.metaCategories);
                    fab.Info.AddMetaCategories(ScreenState.GetRulebookMetacategory());
                }
            }

            return abilities;
        }

        private static bool EncounterValid(EncounterBlueprintData data, List<EncounterPackInfo> packs)
        {
            if (!PackPlugin.Instance.ToggleEncounters)
                return true;

            if (PackPlugin.Instance.EncounterPackManagementBeta)
            {
                return packs.Any(epi => epi.PackContents.Contains(data.name));
            }

            foreach (CardInfo c in data.turns.SelectMany(l => l).Select(cb => cb.card)
                                   .Concat(data.turns.SelectMany(l => l).Select(cb => cb.replacement))
                                   .Concat(data.randomReplacementCards)
                                   .Where(ci => ci != null))
            {
                if (!ActiveCards.Contains(c.name))
                {
                    if (PackPlugin.Instance.RemoveDefaultEncounters || !c.IsBaseGameCard())
                        return false;
                }
            }
            return true;
        }

        private static bool EncounterAllDefault(EncounterBlueprintData data)
        {
            if (PackPlugin.Instance.EncounterPackManagementBeta)
            {
                EncounterPackInfo basePackInfo = GetDefaultPackInfo<EncounterPackInfo>(EncounterPackInfo.GetMostUniqueTempleFromEncounter(data));
                int i = 0;
                foreach (string name in basePackInfo.PackContents)
                {
                    i++;
                    if (name.Equals(data.name, StringComparison.InvariantCultureIgnoreCase))
                        return true;
                }
                if (i > 0)
                    return false;
            }

            foreach (CardInfo c in data.turns.SelectMany(l => l).Select(cb => cb.card)
                                   .Concat(data.turns.SelectMany(l => l).Select(cb => cb.replacement))
                                   .Concat(data.randomReplacementCards)
                                   .Where(ci => ci != null))
            {
                if (!c.IsBaseGameCard())
                    return false;
            }
            return true;
        }

        internal static bool RegionIsValid(RegionData data, List<EncounterPackInfo> packs)
        {
            if (data.name.StartsWith("Midnight") || data.name.Equals("Pirateville"))
                return true;

            return packs.Any(p => p.Regions.Any(r => r.name.Equals(data.name)));
        }

        internal static List<RegionData> FilterEncountersInRegions(List<RegionData> regions)
        {
            if (ShouldFilterCards)
            {
                List<EncounterPackInfo> ActiveEncounterPacks = GetActivePacks<EncounterPackInfo>();
                string packlist = String.Join(", ", ActiveEncounterPacks.Select(p => p.Title));
                PackPlugin.Log.LogInfo($"Filtering regions with packs {packlist}");
                regions.RemoveAll(r => !RegionIsValid(r, ActiveEncounterPacks));
                foreach (RegionData region in regions)
                {
                    int initialCount = region.encounters.Count;
                    List<EncounterBlueprintData> activeBps = region.encounters.Where(bp => EncounterValid(bp, ActiveEncounterPacks)).ToList();
                    if (activeBps.Count == 0 && !PackPlugin.Instance.EncounterPackManagementBeta)
                    {
                        activeBps = region.encounters.Where(bp => EncounterAllDefault(bp)).ToList();
                        PackPlugin.Log.LogInfo($"Region {region.name} started with {initialCount} encounters. Filtered to zero by active packs; resetting to default encounters. There are now {activeBps.Count} encounters.");
                    }
                    else
                    {
                        PackPlugin.Log.LogInfo($"Region {region.name} started with {initialCount} encounters. Filtered to {activeBps.Count} by active packs.");
                    }
                    string encounterList = string.Join(", ", activeBps.Select(bp => bp.name));
                    PackPlugin.Log.LogInfo($"Region {region.name} has these encounters: {encounterList}");
                    region.encounters = activeBps;
                }
            }
            return regions;
        }

        private static bool HasAddedFiltersToEvent = false;
        private static bool ShouldFilterCards = false;

        [HarmonyPatch(typeof(AscensionMenuScreens), nameof(AscensionMenuScreens.TransitionToGame))]
        [HarmonyPrefix]
        internal static void FilterCardAndAbilityList()
        {
            PackPlugin.Log.LogInfo($"Filtering card list");
            if (!HasAddedFiltersToEvent)
            {
                CardManager.ModifyCardList += FilterCardsInPacks;
                AbilityManager.ModifyAbilityList += FilterAbilitiesInPacks;
                RegionManager.ModifyRegionsList += FilterEncountersInRegions;
                HasAddedFiltersToEvent = true;
            }

            ShouldFilterCards = true;
            CardManager.SyncCardList();
            AbilityManager.SyncAbilityList();
            RegionManager.SyncRegionList();
        }

        [HarmonyPatch(typeof(AscensionMenuScreens), nameof(AscensionMenuScreens.Start))]
        [HarmonyPrefix]
        internal static void UnfilterCardAndAbilityList()
        {
            PackPlugin.Log.LogInfo($"Unfiltering card list");

            JSONLoader.LoadFromJSON();
            ForceSyncOfAllPacks();

            ShouldFilterCards = false;
            CardManager.SyncCardList();
            AbilityManager.SyncAbilityList();
            RegionManager.SyncRegionList();
        }
    }
}