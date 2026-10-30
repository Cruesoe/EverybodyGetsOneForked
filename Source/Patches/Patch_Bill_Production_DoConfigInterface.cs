using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Everybody_Gets_One.Patches
{
	[HarmonyPatch(typeof(Bill_Production), "DoConfigInterface")]
	public static class Patch_Bill_Production_DoConfigInterface
	{
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			instructions = OrPersonCountTranspiler.Transpiler(instructions);

			FieldInfo repeatModeInfo = AccessTools.Field(typeof(Bill_Production), nameof(Bill_Production.repeatMode));
			MethodInfo maxInfo = AccessTools.Method(typeof(UnityEngine.Mathf), nameof(UnityEngine.Mathf.Max), new Type[] { typeof(int), typeof(int) });
			FieldInfo targetCountAdjustmentInfo = AccessTools.Field(typeof(RecipeDef), nameof(RecipeDef.targetCountAdjustment));

			foreach (var i in instructions)
			{
				if (i.Calls(maxInfo))
				{
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Ldfld, repeatModeInfo);
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_Bill_Production_DoConfigInterface), nameof(ActuallyMax)));
				}
				else if (i.LoadsField(targetCountAdjustmentInfo))
				{
					yield return i;
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Ldfld, repeatModeInfo);
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_Bill_Production_DoConfigInterface), nameof(ActualTargetCountAdjustment)));
				}
				else
					yield return i;
			}
		}

		// One per person (+ X) has no minimum, so X can go below zero
		public static int ActuallyMax(int a, int b, BillRepeatModeDef def)
		{
			if (def == RepeatModeDefOf.TD_PersonCount) return b;
			return UnityEngine.Mathf.Max(a, b);
		}

		// Recipes like 'go-juice x 4' step by 1 rather than by their product count in these modes
		public static int ActualTargetCountAdjustment(int targetCountAdjustment, BillRepeatModeDef def)
		{
			if (def.IsPerPerson() || def == RepeatModeDefOf.TD_WithSurplusIng)
				return 1;
			return targetCountAdjustment;
		}
	}
}
