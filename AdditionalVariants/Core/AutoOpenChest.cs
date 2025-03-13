using FortRise;
using MonoMod.Utils;
using TowerFall;

namespace AdditionalVariants;

public static class AutoOpenChest
{
    public static void Load()
    {
        On.TowerFall.TreasureChest.Added += Added_patch;
    }

    public static void Unload()
    {
        On.TowerFall.TreasureChest.Added -= Added_patch;
    }

    private static void Added_patch(On.TowerFall.TreasureChest.orig_Added orig, TowerFall.TreasureChest self)
    {
        if (VariantManager.GetCustomVariant("AdditionalVariants/AutoOpenChest"))
        {
            DynamicData.For(self).Set("type", TreasureChest.Types.AutoOpen);
        }
        orig(self);
    }
}