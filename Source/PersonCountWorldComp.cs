using System.Collections.Generic;
using RimWorld.Planet;
using RimWorld;
using Verse;

namespace Everybody_Gets_One
{
	public class PersonCountWorldComp : WorldComponent
	{
		// Bills without their own counter use the colony default
		public PersonCounter defaultCounter = new();
		public Dictionary<Bill_Production, PersonCounter> personCounters = new();

		public PersonCountWorldComp(World world) : base(world) { }

		private List<Bill_Production> scribeBills;
		private List<PersonCounter> scribeCounters;
		public override void ExposeData()
		{
			if (Scribe.mode == LoadSaveMode.Saving)
				personCounters.RemoveAll(kv => kv.Key.billStack == null || kv.Key.DeletedOrDereferenced);

			Scribe_Deep.Look(ref defaultCounter, "defaultCounter");
			// The original mod saved TD Find Library searches under "billPersonCounters"; those are left unread
			Scribe_Collections.Look(ref personCounters, "personCounters", LookMode.Reference, LookMode.Deep, ref scribeBills, ref scribeCounters);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				defaultCounter ??= new();
				personCounters ??= new();
				personCounters.RemoveAll(kv => kv.Key == null || kv.Value == null);
			}
		}

		public bool HasPersonCounter(Bill_Production bill) => personCounters.ContainsKey(bill);

		public PersonCounter PersonCounterOrDefault(Bill_Production bill) =>
			personCounters.TryGetValue(bill, out PersonCounter counter) ? counter : defaultCounter;

		public PersonCounter GetPersonCounter(Bill_Production bill)
		{
			if (!personCounters.TryGetValue(bill, out PersonCounter counter))
			{
				counter = defaultCounter.Clone();
				personCounters[bill] = counter;
			}
			return counter;
		}

		public void SetPersonCounter(Bill_Production bill, PersonCounter counter) => personCounters[bill] = counter;

		public void RemovePersonCounter(Bill_Production bill) => personCounters.Remove(bill);

		public void RemoveCountersOnMap(Map map) =>
			personCounters.RemoveAll(kv => (kv.Key.billStack?.billGiver as Thing)?.MapHeld == map);
	}
}
