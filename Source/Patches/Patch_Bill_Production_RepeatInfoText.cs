using HarmonyLib;
using RimWorld;

namespace Everybody_Gets_One.Patches
{
	[HarmonyPatch(typeof(Bill_Production), nameof(Bill_Production.RepeatInfoText), MethodType.Getter)]
	public static class Patch_Bill_Production_RepeatInfoText
	{
		public static bool Prefix(ref string __result, Bill_Production __instance)
		{
			if (__instance.repeatMode == RepeatModeDefOf.TD_PersonCount)
			{
				__result = $"{__instance.recipe.WorkerCounter.CountProducts(__instance)}/({__instance.CurrentPersonCount()}+{__instance.targetCount})";
				return false;
			}
			if (__instance.repeatMode == RepeatModeDefOf.TD_XPerPerson)
			{
				__result = $"{__instance.recipe.WorkerCounter.CountProducts(__instance)}/{__instance.CurrentPersonCount() * __instance.targetCount} ({__instance.targetCount})";
				return false;
			}
			if (__instance.repeatMode == RepeatModeDefOf.TD_WithSurplusIng)
			{
				__result = $"{__instance.IngredientCount()} > {__instance.targetCount}";
				return false;
			}
			return true;
		}
	}
}
