namespace Teuria.AdditionalVariants;

public class BottomlessQuiver : IHookable
{
    public static void Load()
    {
        On.TowerFall.Player.Added += Added_patch;
    }

    public static void Unload()
    {
        On.TowerFall.Player.Added -= Added_patch;
    }

    private static void Added_patch(On.TowerFall.Player.orig_Added orig, TowerFall.Player self)
    {
        orig(self);
        if (Variants.BottomlessQuiver.IsActive(self.PlayerIndex))
        {
            self.Arrows.SetMaxArrows(int.MaxValue);
        }
    }
}
