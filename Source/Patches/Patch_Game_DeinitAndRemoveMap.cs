using HarmonyLib;
using Verse;

namespace Everybody_Gets_One.Patches
{
	// Bills on a removed map aren't saved, so their counters are dropped with it
	[HarmonyPatch(typeof(Game), nameof(Game.DeinitAndRemoveMap))]
	public static class Patch_Game_DeinitAndRemoveMap
	{
		public static void Prefix(Map map)
		{
			Find.World?.GetComponent<PersonCountWorldComp>()?.RemoveCountersOnMap(map);
		}
	}
}
