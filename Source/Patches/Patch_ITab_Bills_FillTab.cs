using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;

namespace Everybody_Gets_One.Patches
{
	// Pastes the copied person counter once the pasted bill is on a table and has a map
	[HarmonyPatch(typeof(ITab_Bills), "FillTab")]
	public static class Patch_ITab_Bills_FillTab
	{
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			MethodInfo addBillInfo = AccessTools.Method(typeof(BillStack), nameof(BillStack.AddBill));

			foreach (var inst in instructions)
			{
				if (inst.Calls(addBillInfo))
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_ITab_Bills_FillTab), nameof(AddBillAndPasteCounter)));
				else
					yield return inst;
			}
		}

		public static void AddBillAndPasteCounter(BillStack stack, Bill bill)
		{
			stack.AddBill(bill);
			if (Patch_Bill_DoInterface.Clipboard is PersonCounter clip && bill is Bill_Production billP)
				billP.SetPersonCounter(clip.Clone());
		}
	}
}
