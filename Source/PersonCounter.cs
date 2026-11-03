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

		// Filters on humanlike pawns; an empty list accepts anyone
		public bool adults = true;
		public bool children = true;
		public bool babies = true;
		public List<XenotypeDef> xenotypes = new();
		public List<string> customXenotypes = new();
		public List<Ideo> ideos = new();
		public List<ApparelPolicy> apparelPolicies = new();

		// Count cached for one tick and one map
		private int cachedTick = -1;
		private Map cachedMap;
		private int cachedCount;

		public bool Counts(Pawn pawn) => CountsType(pawn) && (!pawn.RaceProps.Humanlike || MatchesFilters(pawn));

		// Cheap checks first: the quest lodger check walks every quest's parts
		private bool CountsType(Pawn pawn)
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

		// Filters for an inactive DLC are ignored
		private bool MatchesFilters(Pawn pawn)
		{
			if (ModsConfig.BiotechActive)
			{
				if (!MatchesAge(pawn.DevelopmentalStage)) return false;
				if (FiltersXenotype && !MatchesXenotype(pawn)) return false;
			}
			if (ModsConfig.IdeologyActive && ideos.Count > 0 && (pawn.Ideo == null || !ideos.Contains(pawn.Ideo))) return false;
			// Only the player's own pawns have an apparel policy; reading it assigns the default when unset
			if (apparelPolicies.Count > 0 && (pawn.Faction != Faction.OfPlayer || pawn.outfits == null || !apparelPolicies.Contains(pawn.outfits.CurrentApparelPolicy))) return false;
			return true;
		}

		private bool MatchesAge(DevelopmentalStage stage) => stage switch
		{
			DevelopmentalStage.Adult => adults,
			DevelopmentalStage.Child => children,
			DevelopmentalStage.Baby or DevelopmentalStage.Newborn => babies,
			_ => true,
		};

		public bool FiltersXenotype => xenotypes.Count > 0 || customXenotypes.Count > 0;

		// Custom xenotypes match by name, the rest by def
		private bool MatchesXenotype(Pawn pawn)
		{
			if (pawn.genes == null) return false;
			return pawn.genes.UniqueXenotype ? customXenotypes.Contains(pawn.genes.xenotypeName) : xenotypes.Contains(pawn.genes.Xenotype);
		}

		// Drops ideoligions and apparel policies that no longer exist
		public void RemoveDeleted()
		{
			if (ideos.Count > 0)
				ideos.RemoveAll(ideo => Find.IdeoManager?.IdeosListForReading.Contains(ideo) != true);
			if (apparelPolicies.Count > 0)
				apparelPolicies.RemoveAll(policy => !Current.Game.outfitDatabase.AllOutfits.Contains(policy));
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
				RemoveDeleted();
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
			clone.xenotypes = new(xenotypes);
			clone.customXenotypes = new(customXenotypes);
			clone.ideos = new(ideos);
			clone.apparelPolicies = new(apparelPolicies);
			clone.ClearCache();
			return clone;
		}

		public bool SameSettings(PersonCounter other) =>
			colonists == other.colonists && slaves == other.slaves && prisoners == other.prisoners && questLodgers == other.questLodgers &&
			mechanoids == other.mechanoids && animals == other.animals && allMaps == other.allMaps &&
			adults == other.adults && children == other.children && babies == other.babies &&
			SameItems(xenotypes, other.xenotypes) && SameItems(customXenotypes, other.customXenotypes) &&
			SameItems(ideos, other.ideos) && SameItems(apparelPolicies, other.apparelPolicies);

		private static bool SameItems<T>(List<T> a, List<T> b) => a.Count == b.Count && a.All(b.Contains);

		public void ExposeData()
		{
			if (Scribe.mode == LoadSaveMode.Saving)
				RemoveDeleted();

			Scribe_Values.Look(ref colonists, "colonists", true);
			Scribe_Values.Look(ref slaves, "slaves");
			Scribe_Values.Look(ref prisoners, "prisoners");
			Scribe_Values.Look(ref questLodgers, "questLodgers");
			Scribe_Values.Look(ref mechanoids, "mechanoids");
			Scribe_Values.Look(ref animals, "animals");
			Scribe_Values.Look(ref allMaps, "allMaps");
			Scribe_Values.Look(ref adults, "adults", true);
			Scribe_Values.Look(ref children, "children", true);
			Scribe_Values.Look(ref babies, "babies", true);
			Scribe_Collections.Look(ref xenotypes, "xenotypes", LookMode.Def);
			Scribe_Collections.Look(ref customXenotypes, "customXenotypes", LookMode.Value);
			Scribe_Collections.Look(ref ideos, "ideos", LookMode.Reference);
			Scribe_Collections.Look(ref apparelPolicies, "apparelPolicies", LookMode.Reference);

			// Entries whose def, ideoligion or policy is gone load as null
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				xenotypes ??= new();
				customXenotypes ??= new();
				ideos ??= new();
				apparelPolicies ??= new();
				xenotypes.RemoveAll(def => def == null);
				customXenotypes.RemoveAll(name => name.NullOrEmpty());
				ideos.RemoveAll(ideo => ideo == null);
				apparelPolicies.RemoveAll(policy => policy == null);
			}
		}
	}
}
