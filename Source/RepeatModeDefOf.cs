using RimWorld;

namespace Everybody_Gets_One
{
	[DefOf]
	public static class RepeatModeDefOf
	{
		public static BillRepeatModeDef TD_PersonCount;
		public static BillRepeatModeDef TD_XPerPerson;
		public static BillRepeatModeDef TD_WithSurplusIng;

		public static bool IsPerPerson(this BillRepeatModeDef mode) => mode == TD_PersonCount || mode == TD_XPerPerson;
	}
}
