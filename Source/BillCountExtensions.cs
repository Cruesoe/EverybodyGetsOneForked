using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Everybody_Gets_One
{
	public static class BillCountExtensions
	{
		// Ingredient counts cached per bill for one tick and one frame
		private static int cacheTick = -1;
		private static int cacheFrame = -1;
		private static readonly Dictionary<Bill_Production, int> ingredientCounts = new();

		public static int TargetCount(this Bill_Production bill)
		{
			return
				bill.repeatMode == RepeatModeDefOf.TD_PersonCount ? bill.CurrentPersonCount() + bill.targetCount :
				bill.repeatMode == RepeatModeDefOf.TD_XPerPerson ? bill.CurrentPersonCount() * bill.targetCount :
				bill.targetCount;
		}

		public static int UnpauseWhenYouHave(this Bill_Production bill)
		{
			return
				bill.repeatMode == RepeatModeDefOf.TD_PersonCount ? bill.CurrentPersonCount() + bill.unpauseWhenYouHave :
				bill.repeatMode == RepeatModeDefOf.TD_XPerPerson ? bill.CurrentPersonCount() * bill.unpauseWhenYouHave :
				bill.unpauseWhenYouHave;
		}

		// Per-person modes need countable products, the surplus mode a single ingredient
		public static bool CanUse(this Bill_Production bill, BillRepeatModeDef mode)
		{
			if (mode.IsPerPerson())
				return bill.recipe.WorkerCounter.CanCountProducts(bill);
			if (mode == RepeatModeDefOf.TD_WithSurplusIng)
				return bill.recipe.ingredients.Count == 1;
			return true;
		}

		public static int IngredientCount(this Bill_Production bill)
		{
			int tick = Find.TickManager.TicksGame;
			if (tick != cacheTick || RealTime.frameCount != cacheFrame)
			{
				ingredientCounts.Clear();
				cacheTick = tick;
				cacheFrame = RealTime.frameCount;
			}
			if (!ingredientCounts.TryGetValue(bill, out int count))
				ingredientCounts[bill] = count = CountIngredient(bill);
			return count;
		}

		// bill.ingredientFilter alone is broader than the recipe allows, so it is intersected with the recipe's fixed filter
		private static int CountIngredient(Bill_Production bill)
		{
			IEnumerable<ThingDef> defs;
			if (bill.recipe.ingredients[0] is IngredientCount ic && ic.IsFixedIngredient)
				defs = new[] { ic.FixedIngredient };
			else
				defs = bill.recipe.fixedIngredientFilter.AllowedThingDefs.Where(bill.ingredientFilter.Allows);

			ResourceCounter resources = bill.Map.resourceCounter;
			int count = 0;
			HashSet<ThingDef> untracked = null;
			foreach (ThingDef def in defs)
			{
				if (def.CountAsResource)
					count += resources.GetCount(def);
				else
					(untracked ??= new HashSet<ThingDef>()).Add(def);
			}

			// Corpses, chunks, weapons and apparel aren't in the resource counter: count them in storage by its rules
			if (untracked != null)
				foreach (SlotGroup group in bill.Map.haulDestinationManager.AllGroupsListForReading)
					foreach (Thing held in group.HeldThings)
					{
						Thing thing = held.GetInnerIfMinified();
						if (untracked.Contains(thing.def) && !thing.IsNotFresh() && !(thing.SpawnedOrAnyParentSpawned && thing.PositionHeld.Fogged(thing.MapHeld)))
							count += thing.stackCount;
					}
			return count;
		}
	}
}
