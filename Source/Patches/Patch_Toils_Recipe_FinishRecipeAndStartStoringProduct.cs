using System.Collections.Generic;
using System.Linq;
using Everybody_Gets_One.Utilities;
using HarmonyLib;
using Verse;
using Verse.AI;

namespace Everybody_Gets_One.Patches
{
	// The finish action refreshes resource counts after a TargetCount bill; this mod's modes get the same refresh
	public static class Patch_Toils_Recipe_FinishRecipeAndStartStoringProduct
	{
		private static int replaced;

		public static void Apply(Harmony harmony)
		{
			string lambdaPrefix = "<" + nameof(Toils_Recipe.FinishRecipeAndStartStoringProduct) + ">";
			harmony.PatchGeneratedMethod(typeof(Toils_Recipe), method => method.Name.StartsWith(lambdaPrefix),
				transpiler: new HarmonyMethod(typeof(Patch_Toils_Recipe_FinishRecipeAndStartStoringProduct), nameof(Transpiler)));
			if (replaced == 0)
				Log.Warning("[Everybody Gets One Forked] Found no resource recount after finishing a bill; per-person and surplus bills may overshoot by one.");
		}

		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> result = OrPersonCountTranspiler.Transpiler(instructions).ToList();
			replaced += result.Count(OrPersonCountTranspiler.IsReplacement);
			return result;
		}
	}
}
