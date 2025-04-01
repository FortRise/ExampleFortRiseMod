using MonoMod.Utils;

namespace MoreReplay;

public static class DarkWorldGameOverPatch 
{
    public static void Load() 
    {
        On.TowerFall.DarkWorldGameOver.ctor += ctor_patch;
    }

    public static void Unload() 
    {
        On.TowerFall.DarkWorldGameOver.ctor -= ctor_patch;
    }

    private static void ctor_patch(On.TowerFall.DarkWorldGameOver.orig_ctor orig, TowerFall.DarkWorldGameOver self, TowerFall.DarkWorldRoundLogic darkWorld)
    {
        orig(self, darkWorld);
        var dynSelf = DynamicData.For(self);
        dynSelf.Set("replaySaved", false);
        dynSelf.Set("saving", false);

        var replayMenuComponent = new ReplayMenuComponent();
        self.Add(replayMenuComponent);
    }
}