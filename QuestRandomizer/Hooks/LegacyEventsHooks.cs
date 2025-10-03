using FortRise;
using HarmonyLib;

namespace Teuria.QuestRandomizer;

[HarmonyPatch(typeof(RiseCore.Events))]
internal class LegacyEventsHooks
{
    [HarmonyPatch("InvokeQuestRoundLogic_PlayerDeath")]
    [HarmonyPrefix]
    public static bool PlayerDeath_Prefix() 
    {
        return false;
    }

    [HarmonyPatch("InvokeQuestRoundLogic_LevelLoadFinish")]
    [HarmonyPrefix]
    public static bool LevelLoadFinish_Prefix() 
    {
        return false;
    }
}

