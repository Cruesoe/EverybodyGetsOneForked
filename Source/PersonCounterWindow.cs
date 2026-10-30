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

		public override Vector2 InitialSize => new Vector2(460f, 460f);

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

		// Options for an inactive DLC stay visible but greyed out, with a tooltip naming the DLC
		private static void Checkbox(Listing_Standard listing, string key, ref bool value, bool available, string requiredPackageId)
		{
			Rect rect = listing.GetRect(Text.LineHeight);
			bool shown = available && value;
			Widgets.CheckboxLabeled(rect, key.Translate(), ref shown, disabled: !available);
			if (available)
				value = shown;
			else
				TooltipHandler.TipRegion(rect, "EGO_RequiresDlc".Translate(ModLister.GetExpansionWithIdentifier(requiredPackageId)?.label ?? requiredPackageId));
			listing.Gap(listing.verticalSpacing);
		}
	}
}
