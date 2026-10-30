using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using HarmonyLib;

namespace Everybody_Gets_One
{

	[HarmonyPatch(typeof(RecipeWorkerCounter), nameof(RecipeWorkerCounter.CountProducts))]
	public static class CountEquippedCountsPersonCounter
	{
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			// FreeColonistsSpawned is who it tracked equipped for.
			MethodInfo FreeColonistsSpawnedInfo = AccessTools.PropertyGetter(typeof(MapPawns), nameof(MapPawns.FreeColonistsSpawned));

			foreach (var inst in instructions)
			{

				if(inst.Calls(FreeColonistsSpawnedInfo))
				{
					yield return new CodeInstruction(OpCodes.Ldarg_1); //bill
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CountEquippedCountsPersonCounter), nameof(FreeColonistsSpawnedOrCountedPawns)));
				}
				else
					yield return inst;
			}
		}

		public static List<Pawn> FreeColonistsSpawnedOrCountedPawns(MapPawns mapPawns, Bill_Production bill)
		{
			if (bill.repeatMode == RepeatModeDefOf.TD_PersonCount || bill.repeatMode == RepeatModeDefOf.TD_XPerPerson)
			{
				//Only return pawns with equipment + apparel + inventory as that is what "Count Equipped" checks.
				return bill.PersonCounterOrDefault().CountedPawns(bill.Map).Where(p => p.equipment != null && p.apparel != null && p.inventory != null).ToList();
			}
			return mapPawns.FreeColonistsSpawned;
		}
	}
}
