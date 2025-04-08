using MonoMod.Utils;
using TowerFall;

namespace Teuria.AdditionalVariants;

public class DrillingArrow : IHookable
{
    public static void Load()
    {
        On.TowerFall.Arrow.OnCollideV += OnCollidedV;
        On.TowerFall.Arrow.OnCollideH += OnCollidedH;
    }

    public static void Unload()
    {
        On.TowerFall.Arrow.OnCollideV -= OnCollidedV;
        On.TowerFall.Arrow.OnCollideH -= OnCollidedH;
    }

    private static void OnCollidedH(On.TowerFall.Arrow.orig_OnCollideH orig, Arrow self, TowerFall.Platform platform)
    {
        if (!CheckDrilled(self, platform))
        {
            orig(self, platform);
        }
    }

    private static void OnCollidedV(On.TowerFall.Arrow.orig_OnCollideV orig, Arrow self, TowerFall.Platform platform)
    {
        if (!CheckDrilled(self, platform))
        {
            orig(self, platform);
        }
    }

    private static bool CheckDrilled(Arrow self, TowerFall.Platform platform) 
    {
        if (self.PlayerIndex >= 0 && Variants.DrillingArrow.IsActive(self.PlayerIndex) && 
            !self.HasDrilled && 
            self.State < Arrow.ArrowStates.Falling && 
            platform is not GraniteBlock)
        {
            var dynSelf = DynamicData.For(self);
            dynSelf.Invoke("StartDrilling");
            return true;
        }
        return false;
    }
}