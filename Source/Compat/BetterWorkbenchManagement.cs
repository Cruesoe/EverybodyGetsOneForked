using System;
using System.Collections.Generic;
using System.Reflection;
using Everybody_Gets_One.Patches;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Everybody_Gets_One.Compat
{
	// Better Workbench Management: its extra bill options and bill mirroring
	[StaticConstructorOnStartup]
	public static class BetterWorkbenchManagement
	{
		static BetterWorkbenchManagement()
		{
			if (ModLister.GetActiveModWithIdentifier("falconne.BWM", ignorePostfix: true) == null)
				return;

			Harmony harmony = new Harmony(EverybodyGetsOneMod.HarmonyId);

			if (AccessTools.TypeByName("ImprovedWorkbenches.BillConfig_DoWindowContents_Patch") is Type patchType &&
				AccessTools.Method(patchType, "DrawFilters") is MethodInfo drawFilters)
				harmony.Patch(drawFilters, transpiler: new HarmonyMethod(typeof(BetterWorkbenchManagement), nameof(Transpiler)));

			if (AccessTools.TypeByName("ImprovedWorkbenches.ExtendedBillDataStorage") is Type storageType &&
				AccessTools.Method(storageType, "MirrorBills") is MethodInfo mirrorBills)
				harmony.Patch(mirrorBills, postfix: new HarmonyMethod(typeof(BetterWorkbenchManagement), nameof(MirrorBillsPostfix)));
		}

		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) =>
			OrPersonCountTranspiler.Transpiler(instructions, noSurplus: true);

		public static void MirrorBillsPostfix(Bill_Production sourceBill, Bill_Production destinationBill)
		{
			if (sourceBill.HasPersonCounter())
				destinationBill.SetPersonCounter(sourceBill.GetPersonCounter().Clone());
			else
				destinationBill.RemovePersonCounter();
		}
	}
}
