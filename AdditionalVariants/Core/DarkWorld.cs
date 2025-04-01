using FortRise;
using MonoMod.Utils;
using TowerFall;

namespace AdditionalVariants;

public static class DarkWorld 
{
    public static void Load() 
    {
        On.TowerFall.Background.Render += DarkerRender;
    }

    public static void Unload() 
    {
        On.TowerFall.Background.Render -= DarkerRender;
    }

    private static void DarkerRender(On.TowerFall.Background.orig_Render orig, TowerFall.Background self)
    {
        if (VariantManager.GetCustomVariant("AdditionalVariants/DarkWorld")) 
        {
            var level = DynamicData.For(self).Get<Level>("level")!;
            var darkened = DynamicData.For(level.OrbLogic).Get<bool>("darkened");
            if (darkened)
                return;
        }
        orig(self);
    }
}