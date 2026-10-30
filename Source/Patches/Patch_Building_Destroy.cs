using HarmonyLib;
using RimWorld;
using Verse;

namespace Everybody_Gets_One.Patches
{
	// Building_WorkTable doesn't override Destroy, so the base method is patched
	[HarmonyPatch(typeof(Building), nameof(Building.Destroy))]
	public static class Patch_Building_Destroy
	{
		public static void Prefix(Building __instance)
		{
			if (__instance is Building_WorkTable workTable)
				foreach (var bill in workTable.BillStack.Bills)
					if (bill is Bill_Production billP)
						billP.RemovePersonCounter();
		}
	}
}
