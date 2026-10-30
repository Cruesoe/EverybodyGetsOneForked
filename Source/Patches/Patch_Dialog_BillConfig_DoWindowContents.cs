using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Everybody_Gets_One.Patches
{
	[HarmonyPatch(typeof(Dialog_BillConfig), nameof(Dialog_BillConfig.DoWindowContents))]
	public static class Patch_Dialog_BillConfig_DoWindowContents
	{
		private static int returnValue;

		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			instructions = OrPersonCountTranspiler.Transpiler(instructions);
			instructions = Transpilers.MethodReplacer(instructions,
				AccessTools.Method(typeof(RecipeWorkerCounter), nameof(RecipeWorkerCounter.CountProducts)),
				AccessTools.Method(typeof(Patch_Dialog_BillConfig_DoWindowContents), nameof(CountTracked)));

			FieldInfo targetCountInfo = AccessTools.Field(typeof(Bill_Production), nameof(Bill_Production.targetCount));
			FieldInfo unpauseWhenYouHaveInfo = AccessTools.Field(typeof(Bill_Production), nameof(Bill_Production.unpauseWhenYouHave));
			FieldInfo targetCountAdjustmentInfo = AccessTools.Field(typeof(RecipeDef), nameof(RecipeDef.targetCountAdjustment));
			FieldInfo billInfo = AccessTools.Field(typeof(Dialog_BillConfig), "bill");
			FieldInfo repeatModeInfo = AccessTools.Field(typeof(Bill_Production), nameof(Bill_Production.repeatMode));

			// The first value load and first address load of targetCount, and the first address load of unpauseWhenYouHave,
			// are the displayed counts; later loads are the X the player edits
			int todoTargetByValue = 1;
			int todoTargetByRef = 1;
			int todoUnpause = 1;
			foreach (CodeInstruction i in instructions)
			{
				if (i.LoadsField(targetCountInfo) && todoTargetByValue-- > 0)
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BillCountExtensions), nameof(BillCountExtensions.TargetCount)));
				else if (i.LoadsField(targetCountInfo, true) && todoTargetByRef-- > 0)
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_Dialog_BillConfig_DoWindowContents), nameof(TargetCountRef)));
				else if (i.LoadsField(unpauseWhenYouHaveInfo, true) && todoUnpause-- > 0)
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_Dialog_BillConfig_DoWindowContents), nameof(UnpauseWhenYouHaveRef)));
				else if (i.LoadsField(targetCountAdjustmentInfo))
				{
					yield return i;
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Ldfld, billInfo);
					yield return new CodeInstruction(OpCodes.Ldfld, repeatModeInfo);
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_Bill_Production_DoConfigInterface), nameof(Patch_Bill_Production_DoConfigInterface.ActualTargetCountAdjustment)));
				}
				else
					yield return i;
			}
		}

		public static int CountTracked(RecipeWorkerCounter counter, Bill_Production bill)
		{
			return bill.repeatMode == RepeatModeDefOf.TD_WithSurplusIng ? bill.IngredientCount() : counter.CountProducts(bill);
		}

		public static ref int TargetCountRef(Bill_Production bill)
		{
			returnValue = bill.TargetCount();
			return ref returnValue;
		}

		public static ref int UnpauseWhenYouHaveRef(Bill_Production bill)
		{
			returnValue = bill.UnpauseWhenYouHave();
			return ref returnValue;
		}
	}
}
