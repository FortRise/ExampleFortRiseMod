using FortRise;
using HarmonyLib;
using MonoMod.Utils;
using TowerFall;

namespace Teuria.AdditionalVariants;

public class DrillingArrow : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Arrow), "OnCollideV"),
            new HarmonyMethod(Arrow_OnCollideVH_Prefix)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Arrow), "OnCollideH"),
            new HarmonyMethod(Arrow_OnCollideVH_Prefix)
        );
    }

    private static bool Arrow_OnCollideVH_Prefix(Arrow __instance, Platform platform)
    {
        return !CheckDrilled(__instance, platform);
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