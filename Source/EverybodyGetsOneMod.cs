using System.Reflection;
using HarmonyLib;
using Verse;

namespace Everybody_Gets_One
{
	public class EverybodyGetsOneMod : Mod
	{
		public EverybodyGetsOneMod(ModContentPack content) : base(content)
		{
			Harmony harmony = new Harmony("cruesoe.everybodygetsoneforked");
			harmony.PatchAll(Assembly.GetExecutingAssembly());
			JobDriver_DoBill_Patch.Apply(harmony);
		}
	}
}
