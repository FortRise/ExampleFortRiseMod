using HarmonyLib;
using TowerFall;

namespace Teuria.Profiles;

[HarmonyPatch(typeof(SessionStats))]
internal static class SessionStatsHooks
{
    [HarmonyPatch(nameof(SessionStats.Initialize))]
    [HarmonyPrefix]
    public static void Initialize_Prefix()
    {
        ProfileSessionStats.Initialize();
    }
}
