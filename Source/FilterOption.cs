using System;

namespace Everybody_Gets_One
{
	// One choice in a person filter's checklist
	public class FilterOption
	{
		public string label;
		public Func<bool> isOn;
		public Action<bool> set;

		public FilterOption(string label, Func<bool> isOn, Action<bool> set)
		{
			this.label = label;
			this.isOn = isOn;
			this.set = set;
		}
	}
}
