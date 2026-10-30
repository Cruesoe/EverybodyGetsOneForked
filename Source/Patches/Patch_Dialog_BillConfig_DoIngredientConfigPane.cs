using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Everybody_Gets_One.Patches
{
	// Adds the "Edit who counts as a person" button above the ingredient pane for per-person bills
	[HarmonyPatch(typeof(Dialog_BillConfig), "DoIngredientConfigPane")]
	public static class Patch_Dialog_BillConfig_DoIngredientConfigPane
	{
		private const float ButtonHeight = 30f;
		private const float Gap = 6f;

		public static void Prefix(Bill_Production ___bill, float x, ref float y, float width, ref float height)
		{
			if (!___bill.repeatMode.IsPerPerson())
				return;

			if (Widgets.ButtonText(new Rect(x, y, width, ButtonHeight), "TD.EditWhoCountsAsAPerson".Translate()))
				Find.WindowStack.Add(new PersonCounterWindow(___bill));

			y += ButtonHeight + Gap;
			height -= ButtonHeight + Gap;
		}
	}
}
