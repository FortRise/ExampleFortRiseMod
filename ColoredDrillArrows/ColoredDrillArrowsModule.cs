using System;
using ColoredDrillArrows.Hooks;
using FortRise;
using MonoMod.ModInterop;

namespace ColoredDrillArrows;

[Fort("com.teuria.ColoredDrillArrows", "ColoredDrillArrows")]
public class ColoredDrillArrowsModule : FortModule
{
    public static ColoredDrillArrowsModule Instance;

    public ColoredDrillArrowsModule() 
    {
        Instance = this;
    }

    public override void LoadContent() {}

    public override void Load()
    {
        DrillArrow.Load();
    }

    public override void Unload()
    {
        DrillArrow.Unload();
    }
}