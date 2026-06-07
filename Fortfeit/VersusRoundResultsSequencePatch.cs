using System.Reflection;
using HarmonyLib;
using MonoMod.Utils;
using TowerFall;

namespace Teuria.Fortfeit;

[HarmonyPatch]
public static class VersusRoundResultsSequencePatch
{
    public static MethodBase TargetMethod()
    {
        return AccessTools.EnumeratorMoveNext(AccessTools.DeclaredMethod(typeof(VersusRoundResults), "Sequence"));
    }

    public static void Postfix(object __instance, in bool __result)
    {
        if (__result)
        {
            return;
        }

        VersusRoundResults realInstance = DynamicData.For(__instance).Get<VersusRoundResults>("<>4__this")!;

        var forfeitButton = new ForfeitButtonGuide(172, "FORFEIT");
        TempLifetime.RoundReults.SetTarget(forfeitButton);
        DynamicData.For(__instance).Set("Teuria.Forfeit::ForfeitButtonGuide", forfeitButton);
        realInstance.Add(forfeitButton);
    }
}
