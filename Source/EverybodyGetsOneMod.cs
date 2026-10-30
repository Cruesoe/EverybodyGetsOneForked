using System.Reflection;
using Everybody_Gets_One.Patches;
using HarmonyLib;
using Verse;

namespace Everybody_Gets_One
{
	public class EverybodyGetsOneMod : Mod
	{
		public const string HarmonyId = "cruesoe.everybodygetsoneforked";

		public EverybodyGetsOneMod(ModContentPack content) : base(content)
		{
			Harmony harmony = new Harmony(HarmonyId);
			harmony.PatchAll(Assembly.GetExecutingAssembly());
			Patch_Toils_Recipe_FinishRecipeAndStartStoringProduct.Apply(harmony);
		}
	}
}
