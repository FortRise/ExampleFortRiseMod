using HarmonyLib;
using TowerFall;

namespace Teuria.QuestRandomizer;

[HarmonyPatch(typeof(QuestTowerStats))]
internal class QuestTowerStatsHooks 
{
    [HarmonyPatch(nameof(QuestTowerStats.BeatNormal))]
    [HarmonyPrefix]
    public static bool BeatNormal_Prefix() 
    {
        return false;
    }

    [HarmonyPatch(nameof(QuestTowerStats.BeatHardcore))]
    [HarmonyPrefix]
    public static bool BeatHardcore_Prefix() 
    {
        return false;
    }
}

