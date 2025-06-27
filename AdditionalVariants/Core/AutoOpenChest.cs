using FortRise;
using HarmonyLib;
using MonoMod.Utils;
using TowerFall;

namespace Teuria.AdditionalVariants;

public class AutoOpenChest : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(
                typeof(TreasureChest),
                nameof(TreasureChest.Added)
            ),
            new HarmonyMethod(TreasureChest_Added_Prefix)
        );
    }

    private static void TreasureChest_Added_Prefix(TreasureChest __instance)
    {
        if (Variants.AutoOpenChest.IsActive())
        {
            DynamicData.For(__instance).Set("type", TreasureChest.Types.AutoOpen);
        }
    }
}