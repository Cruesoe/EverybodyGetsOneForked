using System.Linq;
using RimWorld;
using Verse;

namespace Everybody_Gets_One
{
	public static class BillCountExtensions
	{
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

		public static int IngredientCount(this Bill_Production bill)
		{
			if (bill.recipe.ingredients.First() is IngredientCount ic && ic.IsFixedIngredient)
				return bill.Map.resourceCounter.GetCount(ic.FixedIngredient);
			return bill.Map.resourceCounter.GetCountFor(bill);
		}

		// bill.ingredientFilter alone is broader than the recipe allows, so it is intersected with the recipe's fixed filter
		public static int GetCountFor(this ResourceCounter res, Bill_Production bill)
		{
			return bill.recipe.fixedIngredientFilter.AllowedThingDefs.Sum(def => bill.ingredientFilter.Allows(def) ? res.GetCount(def) : 0);
		}
	}
}
