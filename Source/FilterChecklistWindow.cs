using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Everybody_Gets_One
{
	// Ticks the values one person filter accepts; nothing ticked accepts anyone
	public class FilterChecklistWindow : Window
	{
		private const float RowHeight = 28f;

		private readonly string title;
		private readonly List<FilterOption> options;
		private readonly Action clear;
		private Vector2 scrollPosition;

		public FilterChecklistWindow(string title, List<FilterOption> options, Action clear)
		{
			this.title = title;
			this.options = options;
			this.clear = clear;
			doCloseX = true;
			absorbInputAroundWindow = true;
			closeOnClickedOutside = true;
			onlyOneOfTypeAllowed = true;
		}

		public override Vector2 InitialSize => new Vector2(360f, 520f);

		public override void DoWindowContents(Rect inRect)
		{
			Text.Font = GameFont.Medium;
			Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 35f), title);
			Text.Font = GameFont.Small;

			Rect anyRect = new Rect(inRect.x, inRect.y + 40f, inRect.width, 30f);
			if (Widgets.ButtonText(anyRect, "EGO_FilterAcceptAny".Translate()))
				clear();

			Rect outRect = new Rect(inRect.x, anyRect.yMax + 10f, inRect.width, inRect.yMax - anyRect.yMax - 10f);
			Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, options.Count * RowHeight);
			Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
			for (int i = 0; i < options.Count; i++)
			{
				FilterOption option = options[i];
				bool on = option.isOn();
				bool was = on;
				Widgets.CheckboxLabeled(new Rect(0f, i * RowHeight, viewRect.width, RowHeight), option.label, ref on);
				if (on != was)
					option.set(on);
			}
			Widgets.EndScrollView();
		}
	}
}
