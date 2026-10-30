using HarmonyLib;
using RimWorld;

namespace Everybody_Gets_One.Patches
{
	[HarmonyPatch(typeof(Building_WorkTable), nameof(Building_WorkTable.Notify_BillDeleted))]
	public static class Patch_Building_WorkTable_Notify_BillDeleted
	{
		public static void Postfix(Bill bill)
		{
			if (bill is Bill_Production billP)
				billP.RemovePersonCounter();
		}
	}
}
