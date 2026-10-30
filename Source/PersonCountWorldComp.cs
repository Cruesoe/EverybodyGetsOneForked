using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Everybody_Gets_One
{
	public class PersonCountWorldComp : WorldComponent
	{
		public Dictionary<Bill_Production, PersonCounter> personCounters = new();

		public PersonCountWorldComp(World world) : base(world) { }

		private List<Bill_Production> scribeBills;
		private List<PersonCounter> scribeCounters;
		public override void ExposeData()
		{
			// The original mod saved TD Find Library searches under "billPersonCounters"; those are left unread
			Scribe_Collections.Look(ref personCounters, "personCounters", LookMode.Reference, LookMode.Deep, ref scribeBills, ref scribeCounters);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				personCounters ??= new();
				personCounters.RemoveAll(kv => kv.Key == null || kv.Value == null);
			}
		}

		public bool HasPersonCounter(Bill_Production bill) => personCounters.ContainsKey(bill);

		public void SetPersonCounter(Bill_Production bill, PersonCounter counter) => personCounters[bill] = counter;

		public PersonCounter GetPersonCounter(Bill_Production bill)
		{
			if (!personCounters.TryGetValue(bill, out PersonCounter counter))
			{
				counter = new PersonCounter();
				personCounters[bill] = counter;
			}
			return counter;
		}

		public void RemovePersonCounter(Bill_Production bill) => personCounters.Remove(bill);
	}

	// Loads the original mod's map component from old saves, then removes it
	public class PersonCountMapComp : MapComponent
	{
		public PersonCountMapComp(Map map) : base(map) { }

		public override void FinalizeInit()
		{
			base.FinalizeInit();
			map.components.Remove(this);
		}
	}

	public static class MapCompExtensions
	{
		private static PersonCountWorldComp Comp => Find.World.GetComponent<PersonCountWorldComp>();

		public static int CurrentPersonCount(this Bill_Production bill) =>
			bill.PersonCounterOrDefault().Count(bill.Map);

		public static void OpenPersonCounter(this Bill_Production bill) =>
			Find.WindowStack.Add(new PersonCounterWindow(bill));

		public static bool HasPersonCounter(this Bill_Production bill) => Comp.HasPersonCounter(bill);

		public static PersonCounter GetPersonCounter(this Bill_Production bill) => Comp.GetPersonCounter(bill);

		public static void RemovePersonCounter(this Bill_Production bill) => Comp.RemovePersonCounter(bill);

		public static void SetPersonCounter(this Bill_Production bill, PersonCounter counter) => Comp.SetPersonCounter(bill, counter);

		// Bills without their own counter use the defaults without being added to the save
		private static readonly PersonCounter DefaultCounter = new();
		public static PersonCounter PersonCounterOrDefault(this Bill_Production bill) =>
			bill.HasPersonCounter() ? Comp.GetPersonCounter(bill) : DefaultCounter;
	}

	[HarmonyPatch(typeof(Building_WorkTable), nameof(Building_WorkTable.Notify_BillDeleted))]
	public static class DeleteBillCounter
	{
		public static void Postfix(Bill bill)
		{
			if (bill is Bill_Production billP)
				billP.RemovePersonCounter();
		}
	}

	[HarmonyPatch(typeof(Building), nameof(Building.Destroy))]
	public class Building_Destroy_Detour
	{
		public static void Prefix(Building __instance)
		{
			//Because Building_WorkTable doesn't override Destroy
			if (__instance is Building_WorkTable workTable)
				foreach (var bill in workTable.BillStack.Bills)
					if (bill is Bill_Production billP)
						billP.RemovePersonCounter();
		}
	}
}
