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

		// Count cached for one tick and one map
		private int cachedTick = -1;
		private Map cachedMap;
		private int cachedCount;

		// Cheap checks first: the quest lodger check walks every quest's parts
		public bool Counts(Pawn pawn)
		{
			if (pawn.IsPrisonerOfColony) return prisoners;
			if (pawn.Faction != Faction.OfPlayer) return false;
			if (pawn.IsSlaveOfColony) return slaves;
			if (pawn.IsColonyMech) return mechanoids;
			if (pawn.RaceProps.Animal) return animals;
			if (!pawn.IsColonist) return false;
			if (pawn.IsQuestLodger()) return questLodgers;
			return colonists;
		}

		public IEnumerable<Pawn> CountedPawns(Map billMap)
		{
			IEnumerable<Map> maps = allMaps ? Find.Maps : billMap != null ? new[] { billMap } : Enumerable.Empty<Map>();
			return maps.SelectMany(map => map.mapPawns.AllPawnsSpawned).Where(Counts);
		}

		public int Count(Map billMap)
		{
			int tick = Find.TickManager.TicksGame;
			if (tick != cachedTick || billMap != cachedMap)
			{
				cachedCount = CountedPawns(billMap).Count();
				cachedTick = tick;
				cachedMap = billMap;
			}
			return cachedCount;
		}

		public void ClearCache() => cachedTick = -1;

		public PersonCounter Clone()
		{
			PersonCounter clone = (PersonCounter)MemberwiseClone();
			clone.ClearCache();
			return clone;
		}

		public bool SameSettings(PersonCounter other) =>
			colonists == other.colonists && slaves == other.slaves && prisoners == other.prisoners && questLodgers == other.questLodgers &&
			mechanoids == other.mechanoids && animals == other.animals && allMaps == other.allMaps;

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
