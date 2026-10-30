using HarmonyLib;
using RimWorld;

namespace Everybody_Gets_One.Patches
{
	[HarmonyPatch(typeof(Bill_Production), nameof(Bill_Production.ShouldDoNow))]
	public static class Patch_Bill_Production_ShouldDoNow
	{
		public static bool Prefix(ref bool __result, Bill_Production __instance)
		{
			if (__instance.repeatMode.IsPerPerson())
			{
				if (__instance.suspended)
				{
					__result = false;
					return false;
				}

				// Same as the TargetCount mode, with the per-person target
				int products = __instance.recipe.WorkerCounter.CountProducts(__instance);
				int targetCount = __instance.TargetCount();
				if (__instance.pauseWhenSatisfied && products >= targetCount)
					__instance.paused = true;
				if (products <= __instance.UnpauseWhenYouHave() || !__instance.pauseWhenSatisfied)
					__instance.paused = false;
				__result = !__instance.paused && products < targetCount;
				return false;
			}
			if (__instance.repeatMode == RepeatModeDefOf.TD_WithSurplusIng)
			{
				// Vanilla clears the pause for every mode except TargetCount
				__instance.paused = false;

				if (__instance.suspended)
					__result = false;
				// Finish a bound unfinished thing even if taking its ingredient dropped the count below the target
				else if (__instance is Bill_ProductionWithUft billUft && billUft.BoundUft != null)
					__result = true;
				else
					__result = __instance.IngredientCount() > __instance.targetCount;
				return false;
			}
			return true;
		}
	}
}
