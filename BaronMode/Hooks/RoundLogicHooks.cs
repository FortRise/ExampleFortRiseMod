
using FortRise;
using HarmonyLib;
using Teuria.BaronMode.GameModes;
using TowerFall;

namespace Teuria.BaronMode.Hooks;

public class RoundLogicHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(RoundLogic), nameof(RoundLogic.FFACheckForAllButOneDead)),
            new HarmonyMethod(RoundLogic_FFACheckForAllButOneDead_Prefix)
        );
    }

    private static bool RoundLogic_FFACheckForAllButOneDead_Prefix(RoundLogic __instance, ref bool __result)
    {
        if (__instance is BaronRoundLogic)
        {
            __result = false;
            return false;
        }

        return true;
    }
}