using System;
using System.Collections.Generic;
using DiskCardGame;
using UnityEngine;

namespace Infiniscryption.Curses.Patchers
{
	public class DeathcardGenerator
	{
		public static CardModificationInfo GenerateMod(int hauntLevel)
		{
			List<AbilityInfo> validAbilities = ScriptableObjectLoader<AbilityInfo>.AllData.FindAll((AbilityInfo x) => x.metaCategories.Contains(AbilityMetaCategory.Part1Modular) && x.opponentUsable);
			int statPoints = 3 + hauntLevel + 2 * RunState.Run.regionTier;

			CardModificationInfo cardModificationInfo = CardInfoGenerator.CreateRandomizedAbilitiesStatsMod(validAbilities, statPoints, 1, 2);
			int seed = SaveManager.saveFile.GetCurrentRandomSeed()+110;
            CompositeFigurine.FigurineType head = (CompositeFigurine.FigurineType)SeededRandom.Range(0, (int)CompositeFigurine.FigurineType.NUM_FIGURINES, seed++);
            if (SeededRandom.Value(seed++) < 0.2)
                cardModificationInfo.deathCardInfo = new (head, true);
            else
                cardModificationInfo.deathCardInfo = new (head, SeededRandom.Range(0, 6, seed++), SeededRandom.Range(0, 6, seed++));

            cardModificationInfo.nameReplacement = DEATHCARD_NAMES[SeededRandom.Range(0, DEATHCARD_NAMES.Length, seed++)];
			return cardModificationInfo;
		}

		private static readonly string[] DEATHCARD_NAMES = new string[] {
            "Aaron",
            "Mad Hatter",
            "Julian",
            "Joseph",
            "Ingo",
            "Green",
            "Never Named",
            "Matzie",
            "Lily",
            "Deer Sir",
            "Gary",
            "Ara",
            "Bitty",
            "Papi",
            "Hayper",
            "Slime",
            "Blind",
            "Kek",
            "Cactus",
            "xXxStoner420BongMasterxXx",
            "Void",
            "Warrior",
            "Spritz",
            "Penguin",
            "Jury",
            "Thunder"
        };
	}
}
