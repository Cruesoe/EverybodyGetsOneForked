using Verse;

namespace Everybody_Gets_One
{
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
}
