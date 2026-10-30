using RimWorld;
using Verse;

namespace Everybody_Gets_One
{
	public static class PersonCounterExtensions
	{
		private static PersonCountWorldComp Comp => Find.World.GetComponent<PersonCountWorldComp>();

		public static int CurrentPersonCount(this Bill_Production bill) => bill.PersonCounterOrDefault().Count(bill.Map);

		public static PersonCounter PersonCounterOrDefault(this Bill_Production bill) => Comp.PersonCounterOrDefault(bill);

		public static bool HasPersonCounter(this Bill_Production bill) => Comp.HasPersonCounter(bill);

		public static PersonCounter GetPersonCounter(this Bill_Production bill) => Comp.GetPersonCounter(bill);

		public static void SetPersonCounter(this Bill_Production bill, PersonCounter counter) => Comp.SetPersonCounter(bill, counter);

		public static void RemovePersonCounter(this Bill_Production bill) => Comp.RemovePersonCounter(bill);
	}
}
