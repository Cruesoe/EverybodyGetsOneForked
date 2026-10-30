using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Everybody_Gets_One
{
	// Which pawns a bill counts as people; the defaults match the original mod's default search
	public class PersonCounter : IExposable
	{
		public bool colonists = true;
		public bool slaves;
		public bool prisoners;
		public bool questLodgers;
		public bool mechanoids;
		public bool animals;
		public bool allMaps;

		public bool Counts(Pawn pawn)
		{
			if (pawn.IsQuestLodger()) return questLodgers;
			if (pawn.IsSlaveOfColony) return slaves;
			if (pawn.IsPrisonerOfColony) return prisoners;
			if (pawn.IsColonyMech) return mechanoids;
			if (pawn.IsColonist) return colonists;
			if (pawn.Faction == Faction.OfPlayer && pawn.RaceProps.Animal) return animals;
			return false;
		}

		public IEnumerable<Pawn> CountedPawns(Map billMap)
		{
			IEnumerable<Map> maps = allMaps ? Find.Maps : billMap != null ? new[] { billMap } : Enumerable.Empty<Map>();
			return maps.SelectMany(map => map.mapPawns.AllPawnsSpawned).Where(Counts);
		}

		public int Count(Map billMap) => CountedPawns(billMap).Count();

		public PersonCounter Clone() => (PersonCounter)MemberwiseClone();

		public void ExposeData()
		{
			Scribe_Values.Look(ref colonists, "colonists", true);
			Scribe_Values.Look(ref slaves, "slaves");
			Scribe_Values.Look(ref prisoners, "prisoners");
			Scribe_Values.Look(ref questLodgers, "questLodgers");
			Scribe_Values.Look(ref mechanoids, "mechanoids");
			Scribe_Values.Look(ref animals, "animals");
			Scribe_Values.Look(ref allMaps, "allMaps");
		}
	}
}
