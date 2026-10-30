using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Everybody_Gets_One.Patches
{
	[HarmonyPatch(typeof(BillRepeatModeUtility), nameof(BillRepeatModeUtility.MakeConfigFloatMenu))]
	public static class Patch_BillRepeatModeUtility_MakeConfigFloatMenu
	{
		// Adds the modes to the option list right after it is created
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			ConstructorInfo listCtorInfo = AccessTools.Constructor(typeof(List<FloatMenuOption>), new Type[] { });

			foreach (CodeInstruction i in instructions)
			{
				yield return i;
				if (i.opcode == OpCodes.Newobj && i.operand.Equals(listCtorInfo))
				{
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_BillRepeatModeUtility_MakeConfigFloatMenu), nameof(InsertModes)));
				}
			}
		}

		public static List<FloatMenuOption> InsertModes(List<FloatMenuOption> options, Bill_Production bill)
		{
			options.Add(new FloatMenuOption(RepeatModeDefOf.TD_PersonCount.LabelCap, () => SetCountedMode(bill, RepeatModeDefOf.TD_PersonCount)));
			options.Add(new FloatMenuOption(RepeatModeDefOf.TD_XPerPerson.LabelCap, () => SetCountedMode(bill, RepeatModeDefOf.TD_XPerPerson)));
			options.Add(new FloatMenuOption(RepeatModeDefOf.TD_WithSurplusIng.LabelCap, delegate
			{
				if (bill.recipe.ingredients.Count != 1)
					Messages.Message("TD.RecipeCannotHaveSurplus".Translate(), MessageTypeDefOf.RejectInput, false);
				else
					bill.repeatMode = RepeatModeDefOf.TD_WithSurplusIng;
			}));
			return options;
		}

		private static void SetCountedMode(Bill_Production bill, BillRepeatModeDef mode)
		{
			if (!bill.recipe.WorkerCounter.CanCountProducts(bill))
				Messages.Message("RecipeCannotHaveTargetCount".Translate(), MessageTypeDefOf.RejectInput, false);
			else
				bill.repeatMode = mode;
		}
	}
}
