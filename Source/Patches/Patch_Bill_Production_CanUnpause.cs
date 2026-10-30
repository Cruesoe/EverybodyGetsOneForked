using HarmonyLib;
using RimWorld;

namespace Everybody_Gets_One.Patches
{
	[HarmonyPatch(typeof(Bill_Production), "CanUnpause")]
	public static class Patch_Bill_Production_CanUnpause
	{
		public static bool Prefix(ref bool __result, Bill_Production __instance)
		{
			if (__instance.repeatMode.IsPerPerson())
			{
				__result = __instance.paused && __instance.pauseWhenSatisfied && __instance.recipe.WorkerCounter.CountProducts(__instance) < __instance.TargetCount();
				return false;
			}
			return true;
		}
	}
}
