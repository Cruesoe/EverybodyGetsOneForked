using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Everybody_Gets_One
{
	public class PersonCounterWindow : Window
	{
		private readonly Bill_Production bill;

		public PersonCounterWindow(Bill_Production bill)
		{
			this.bill = bill;
			forcePause = true;
			doCloseX = true;
			absorbInputAroundWindow = true;
			closeOnClickedOutside = true;
			onlyOneOfTypeAllowed = true;
		}

		public override Vector2 InitialSize => new Vector2(460f, 420f);

		public override void DoWindowContents(Rect inRect)
		{
			PersonCounter counter = bill.GetPersonCounter();

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

			listing.GapLine();
			listing.Label("EGO_CountedNow".Translate(counter.Count(bill.Map)));

			listing.Gap();
			if (listing.ButtonText("EGO_ResetCounter".Translate()))
				bill.RemovePersonCounter();

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

	[HarmonyPatch(typeof(Dialog_BillConfig), "DoIngredientConfigPane")]
	public static class InsertPersonCounterButton
	{
		//protected virtual void DoIngredientConfigPane(float x, ref float y, float width, float height)
		public static void Prefix(Bill_Production ___bill, float x, ref float y, float width, ref float height)
		{
			if (___bill.repeatMode == RepeatModeDefOf.TD_PersonCount || ___bill.repeatMode == RepeatModeDefOf.TD_XPerPerson)
			{
				Rect rect = new(x, y, width, 30);
				if (Widgets.ButtonText(rect, "TD.EditWhoCountsAsAPerson".Translate()))
					___bill.OpenPersonCounter();

				y += 30 + 6;
				height -= 30 + 6;
			}
		}
	}
}
