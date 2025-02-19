using System;
using FortRise;
using Monocle;
using MonoMod.Utils;
using TowerFall;

namespace ColoredDrillArrows.Hooks;

public static class DrillArrow
{
    public static void Load()
    {
        On.TowerFall.DrillArrow.InitGraphics += InitGraphics_patch;
    }

    public static void Unload()
    {
        On.TowerFall.DrillArrow.InitGraphics -= InitGraphics_patch;
    }

    private static void InitGraphics_patch(On.TowerFall.DrillArrow.orig_InitGraphics orig, TowerFall.DrillArrow self)
    {
        orig(self);
        var dynSelf = DynamicData.For(self);
        var normalSprite = dynSelf.Get<Sprite<int>>("normalSprite");
        var buriedImage = dynSelf.Get<Image>("buriedImage");
        if (self.CharacterIndex != -1)
        {
            normalSprite.Color = ArcherData.Archers[self.CharacterIndex].ColorB;
            buriedImage.Color = ArcherData.Archers[self.CharacterIndex].ColorB;
        }
    }
}