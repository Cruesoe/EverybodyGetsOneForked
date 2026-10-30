using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;

namespace Everybody_Gets_One.Patches
{
	// Copying a bill also copies its person counter; Patch_ITab_Bills_FillTab pastes it
	[HarmonyPatch(typeof(Bill), nameof(Bill.DoInterface))]
	public static class Patch_Bill_DoInterface
	{
		public static PersonCounter Clipboard;

		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			FieldInfo clipboardInfo = AccessTools.Field(typeof(BillUtility), nameof(BillUtility.Clipboard));

			foreach (var inst in instructions)
			{
				yield return inst;

				if (inst.StoresField(clipboardInfo))
				{
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_Bill_DoInterface), nameof(CopyCounterFor)));
				}
			}
		}

		public static void CopyCounterFor(Bill bill)
		{
			Clipboard = bill is Bill_Production billP && billP.HasPersonCounter() ? billP.GetPersonCounter().Clone() : null;
		}
	}
}
