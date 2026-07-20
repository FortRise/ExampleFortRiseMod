using FortRise;
using HarmonyLib;
using MonoMod.Utils;
using TowerFall;

namespace Teuria.MoreReplay;

public sealed class TrialsResultPatch : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredConstructor(
                typeof(TrialsResults), 
                [
                    typeof(Session),
                    typeof(long),
                    typeof(bool),
                    typeof(bool),
                    typeof(bool),
                    typeof(bool),
                    typeof(long)
                ]),
            postfix: new HarmonyMethod(TrialsResults_ctor_Prefix)
        );
    }

    private static void TrialsResults_ctor_Prefix(TrialsStart __instance)
    {
        var dynSelf = DynamicData.For(__instance);
        dynSelf.Set("replaySaved", false);
        dynSelf.Set("saving", false);

        var replayMenuComponent = new ReplayMenuComponent(true, false);
        __instance.Add(replayMenuComponent);
    }
}


