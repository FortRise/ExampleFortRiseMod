using FortRise;
using HarmonyLib;
using MonoMod.Utils;
using TowerFall;

namespace Teuria.MoreReplay;

public class DarkWorldGameOverPatch : IHookable 
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredConstructor(typeof(DarkWorldGameOver), [typeof(DarkWorldRoundLogic)]),
            postfix: new HarmonyMethod(DarkWorldGameOver_ctor_Postfix)
        );
    }

    private static void DarkWorldGameOver_ctor_Postfix(TowerFall.DarkWorldGameOver __instance)
    {
        var dynSelf = DynamicData.For(__instance);
        dynSelf.Set("replaySaved", false);
        dynSelf.Set("saving", false);

        var replayMenuComponent = new ReplayMenuComponent();
        __instance.Add(replayMenuComponent);
    }
}