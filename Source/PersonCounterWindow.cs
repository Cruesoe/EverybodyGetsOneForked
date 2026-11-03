using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Everybody_Gets_One
{
	// Edits who a bill counts; a bill gets its own counter only once its settings differ from the default
	public class PersonCounterWindow : Window
	{
		private readonly Bill_Production bill;
		private PersonCounter counter;

		public PersonCounterWindow(Bill_Production bill)
		{
			this.bill = bill;
			counter = bill.HasPersonCounter() ? bill.GetPersonCounter() : bill.PersonCounterOrDefault().Clone();
			forcePause = true;
			doCloseX = true;
			absorbInputAroundWindow = true;
			closeOnClickedOutside = true;
			onlyOneOfTypeAllowed = true;
		}

		private static PersonCountWorldComp Comp => Find.World.GetComponent<PersonCountWorldComp>();

		public override Vector2 InitialSize => new Vector2(460f, 690f);

		public override void DoWindowContents(Rect inRect)
		{
			Listing_Standard listing = new Listing_Standard();
			listing.Begin(inRect);

			Text.Font = GameFont.Medium;
			listing.Label("EGO_PersonCounterTitle".Translate(bill.LabelCap));
			Text.Font = GameFont.Small;
			listing.GapLine();

			Checkbox(listing, "EGO_CountColonists", ref counter.colonists, true, null);
			Checkbox(listing, "EGO_CountSlaves", ref counter.slaves, ModsConfig.IdeologyActive, ModContentPack.IdeologyModPackageId);
			Checkbox(listing, "EGO_CountPrisoners", ref counter.prisoners, true, null);
			Checkbox(listing, "EGO_CountQuestLodgers", ref counter.questLodgers, true, null);
			Checkbox(listing, "EGO_CountMechanoids", ref counter.mechanoids, ModsConfig.BiotechActive, ModContentPack.BiotechModPackageId);
			Checkbox(listing, "EGO_CountAnimals", ref counter.animals, true, null);

			listing.GapLine();
			listing.Label("EGO_FiltersHeader".Translate(), tooltip: "EGO_FiltersHeaderTip".Translate());
			Rect ageRow = listing.GetRect(Text.LineHeight);
			float third = ageRow.width / 3f;
			CheckboxIn(ageRow.LeftPartPixels(third - 10f), "EGO_CountAdults", ref counter.adults, ModsConfig.BiotechActive, ModContentPack.BiotechModPackageId, true);
			CheckboxIn(new Rect(ageRow.x + third, ageRow.y, third - 10f, ageRow.height), "EGO_CountChildren", ref counter.children, ModsConfig.BiotechActive, ModContentPack.BiotechModPackageId, true);
			CheckboxIn(new Rect(ageRow.x + 2f * third, ageRow.y, third - 10f, ageRow.height), "EGO_CountBabies", ref counter.babies, ModsConfig.BiotechActive, ModContentPack.BiotechModPackageId, true);
			listing.Gap(listing.verticalSpacing);
			FilterButton(listing, "EGO_FilterXenotype", counter.xenotypes.Count + counter.customXenotypes.Count,
				() => counter.xenotypes.Count > 0 ? counter.xenotypes[0].LabelCap.Resolve() : counter.customXenotypes[0].CapitalizeFirst(),
				XenotypeOptions, () => { counter.xenotypes.Clear(); counter.customXenotypes.Clear(); }, ModsConfig.BiotechActive, ModContentPack.BiotechModPackageId, null);
			FilterButton(listing, "EGO_FilterIdeo", counter.ideos.Count, () => counter.ideos[0].name.CapitalizeFirst(),
				IdeoOptions, () => counter.ideos.Clear(), ModsConfig.IdeologyActive, ModContentPack.IdeologyModPackageId, null);
			FilterButton(listing, "EGO_FilterApparelPolicy", counter.apparelPolicies.Count, () => counter.apparelPolicies[0].label,
				ApparelPolicyOptions, () => counter.apparelPolicies.Clear(), true, null, "EGO_FilterApparelPolicyTip".Translate());

			listing.GapLine();
			if (listing.RadioButton("EGO_CountThisMap".Translate(), !counter.allMaps))
				counter.allMaps = false;
			if (listing.RadioButton("EGO_CountAllMaps".Translate(), counter.allMaps))
				counter.allMaps = true;

			if (!bill.HasPersonCounter() && !counter.SameSettings(Comp.defaultCounter))
				bill.SetPersonCounter(counter);
			counter.ClearCache();

			listing.GapLine();
			listing.Label("EGO_CountedNow".Translate(counter.Count(bill.Map)));

			listing.Gap();
			if (listing.ButtonText("EGO_SetAsDefault".Translate(), "EGO_SetAsDefaultTip".Translate()))
			{
				Comp.defaultCounter = counter.Clone();
				bill.RemovePersonCounter();
			}
			if (listing.ButtonText("EGO_ResetCounter".Translate()))
			{
				bill.RemovePersonCounter();
				counter = Comp.defaultCounter.Clone();
			}

			listing.End();
		}

		private List<FilterOption> XenotypeOptions()
		{
			List<FilterOption> options = DefDatabase<XenotypeDef>.AllDefs.OrderBy(def => def.label)
				.Select(def => new FilterOption(def.LabelCap, () => counter.xenotypes.Contains(def), on => Toggle(counter.xenotypes, def, on)))
				.ToList();
			// Custom xenotypes: saved designs, ones carried by pawns on any map, and ones already picked
			IEnumerable<string> customNames = Current.Game.customXenotypeDatabase.customXenotypes.Select(custom => custom.name)
				.Concat(Find.Maps.SelectMany(map => map.mapPawns.AllPawnsSpawned).Where(pawn => pawn.genes != null && pawn.genes.UniqueXenotype).Select(pawn => pawn.genes.xenotypeName))
				.Concat(counter.customXenotypes)
				.Where(name => !name.NullOrEmpty())
				.Distinct()
				.OrderBy(name => name);
			options.AddRange(customNames.Select(name => new FilterOption(name.CapitalizeFirst(), () => counter.customXenotypes.Contains(name), on => Toggle(counter.customXenotypes, name, on))));
			return options;
		}

		private List<FilterOption> IdeoOptions() =>
			Find.IdeoManager.IdeosInViewOrder
				.Select(ideo => new FilterOption(ideo.name.CapitalizeFirst(), () => counter.ideos.Contains(ideo), on => Toggle(counter.ideos, ideo, on)))
				.ToList();

		private List<FilterOption> ApparelPolicyOptions() =>
			Current.Game.outfitDatabase.AllOutfits
				.Select(policy => new FilterOption(policy.label, () => counter.apparelPolicies.Contains(policy), on => Toggle(counter.apparelPolicies, policy, on)))
				.ToList();

		private static void Toggle<T>(List<T> list, T item, bool on)
		{
			if (on && !list.Contains(item))
				list.Add(item);
			else if (!on)
				list.Remove(item);
		}

		// A labelled button showing what the filter accepts; it opens the filter's checklist, whose options are built only then
		private static void FilterButton(Listing_Standard listing, string key, int selectedCount, Func<string> singleLabel, Func<List<FilterOption>> getOptions, Action clear, bool available, string requiredPackageId, string tooltip)
		{
			Rect rect = listing.GetRect(30f);
			Widgets.Label(rect.LeftPart(0.4f), key.Translate());
			Rect buttonRect = rect.RightPart(0.6f);
			string summary = !available || selectedCount == 0 ? "EGO_FilterAny".Translate().Resolve() : selectedCount == 1 ? singleLabel() : "EGO_FilterSelected".Translate(selectedCount).Resolve();
			if (Widgets.ButtonText(buttonRect, summary, active: available) && available)
				Find.WindowStack.Add(new FilterChecklistWindow(key.Translate(), getOptions(), clear));
			if (!available)
				TooltipHandler.TipRegion(rect, RequiresDlc(requiredPackageId));
			else if (tooltip != null)
				TooltipHandler.TipRegion(rect, tooltip);
			listing.Gap(listing.verticalSpacing);
		}

		private static void Checkbox(Listing_Standard listing, string key, ref bool value, bool available, string requiredPackageId)
		{
			CheckboxIn(listing.GetRect(Text.LineHeight), key, ref value, available, requiredPackageId);
			listing.Gap(listing.verticalSpacing);
		}

		// Options for an inactive DLC stay visible but greyed out, showing unavailableValue, with a tooltip naming the DLC
		private static void CheckboxIn(Rect rect, string key, ref bool value, bool available, string requiredPackageId, bool unavailableValue = false)
		{
			bool shown = available ? value : unavailableValue;
			Widgets.CheckboxLabeled(rect, key.Translate(), ref shown, disabled: !available);
			if (available)
				value = shown;
			else
				TooltipHandler.TipRegion(rect, RequiresDlc(requiredPackageId));
		}

		private static string RequiresDlc(string packageId) =>
			"EGO_RequiresDlc".Translate(ModLister.GetExpansionWithIdentifier(packageId)?.label ?? packageId);
	}
}
