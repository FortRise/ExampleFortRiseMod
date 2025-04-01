using ColoredDrillArrows.Hooks;
using FortRise;

namespace ColoredDrillArrows;

[Fort("com.teuria.ColoredDrillArrows", "ColoredDrillArrows")]
public class ColoredDrillArrowsModule : FortModule
{
    public static ColoredDrillArrowsModule Instance = null!;

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