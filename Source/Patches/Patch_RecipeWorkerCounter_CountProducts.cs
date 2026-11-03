using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Everybody_Gets_One.Patches
{
	// "Count equipped" checks the bill's counted people instead of the map's free colonists for per-person bills
	[HarmonyPatch(typeof(RecipeWorkerCounter), nameof(RecipeWorkerCounter.CountProducts))]
	public static class Patch_RecipeWorkerCounter_CountProducts
	{
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			MethodInfo freeColonistsSpawnedInfo = AccessTools.PropertyGetter(typeof(MapPawns), nameof(MapPawns.FreeColonistsSpawned));

			foreach (var inst in instructions)
			{
				if (inst.Calls(freeColonistsSpawnedInfo))
				{
					yield return new CodeInstruction(OpCodes.Ldarg_1);
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_RecipeWorkerCounter_CountProducts), nameof(FreeColonistsSpawnedOrCountedPawns)));
				}
				else
					yield return inst;
			}
		}

		public static List<Pawn> FreeColonistsSpawnedOrCountedPawns(MapPawns mapPawns, Bill_Production bill)
		{
			if (bill.repeatMode.IsPerPerson())
				return bill.PersonCounterOrDefault().EquippablePawns(bill.Map);
			return mapPawns.FreeColonistsSpawned;
		}
	}
}
