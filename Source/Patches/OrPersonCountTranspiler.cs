using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;

namespace Everybody_Gets_One.Patches
{
	// Rewrites `repeatMode == BillRepeatModeDefOf.TargetCount` checks to also accept this mod's modes
	public static class OrPersonCountTranspiler
	{
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) =>
			Transpiler(instructions, false);

		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, bool noSurplus)
		{
			FieldInfo repeatModeInfo = AccessTools.Field(typeof(Bill_Production), nameof(Bill_Production.repeatMode));
			FieldInfo targetCountInfo = AccessTools.Field(typeof(BillRepeatModeDefOf), nameof(BillRepeatModeDefOf.TargetCount));

			List<CodeInstruction> instList = instructions.ToList();
			for (int i = 0; i < instList.Count; i++)
			{
				CodeInstruction inst = instList[i];

				// ldfld Bill_Production::repeatMode, ldsfld BillRepeatModeDefOf::TargetCount, bne.un/beq
				if (i >= 2 &&
					(inst.opcode == OpCodes.Bne_Un || inst.opcode == OpCodes.Beq || inst.opcode == OpCodes.Bne_Un_S || inst.opcode == OpCodes.Beq_S) &&
					instList[i - 2].LoadsField(repeatModeInfo) &&
					instList[i - 1].LoadsField(targetCountInfo))
				{
					// The next string loaded tells the pause-when-satisfied check apart from the target count check
					string context = instList.Skip(i + 1).FirstOrDefault(k => k.opcode == OpCodes.Ldstr)?.operand as string;

					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(OrPersonCountTranspiler),
						noSurplus || context == "PauseWhenSatisfied" ? nameof(IsAnyPauseMode) : nameof(IsAnyTargetMode)));
					yield return new CodeInstruction(
						inst.opcode == OpCodes.Bne_Un ? OpCodes.Brfalse :
						inst.opcode == OpCodes.Bne_Un_S ? OpCodes.Brfalse_S :
						inst.opcode == OpCodes.Beq ? OpCodes.Brtrue :
						OpCodes.Brtrue_S,
						inst.operand);
				}
				else
					yield return inst;
			}
		}

		public static bool IsReplacement(CodeInstruction inst) =>
			inst.Calls(AccessTools.Method(typeof(OrPersonCountTranspiler), nameof(IsAnyTargetMode))) ||
			inst.Calls(AccessTools.Method(typeof(OrPersonCountTranspiler), nameof(IsAnyPauseMode)));

		public static bool IsAnyTargetMode(BillRepeatModeDef repeatMode, BillRepeatModeDef targetCountMode) =>
			repeatMode == targetCountMode || repeatMode.IsPerPerson() || repeatMode == RepeatModeDefOf.TD_WithSurplusIng;

		public static bool IsAnyPauseMode(BillRepeatModeDef repeatMode, BillRepeatModeDef targetCountMode) =>
			repeatMode == targetCountMode || repeatMode.IsPerPerson();
	}
}
