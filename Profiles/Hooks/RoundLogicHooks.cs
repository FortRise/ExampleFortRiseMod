using HarmonyLib;
using TowerFall;

namespace Teuria.Profiles;

[HarmonyPatch(typeof(RoundLogic))]
internal static class RoundLogicHooks
{
    [HarmonyPatch(nameof(RoundLogic.OnPlayerDeath))]
    [HarmonyPrefix]
    public static void RoundLogic_OnPlayerDeath_Postfix(RoundLogic __instance, int playerIndex, int killerIndex)
    {
        DeathType deathType = DeathType.Normal;
        if (killerIndex == playerIndex)
        {
            deathType = DeathType.Self;
        }
        else if (killerIndex != -1 && 
            __instance.Session.MatchSettings.TeamMode && 
            __instance.Session.MatchSettings.GetPlayerAllegiance(playerIndex) == __instance.Session.MatchSettings.GetPlayerAllegiance(killerIndex))
        {
            deathType = DeathType.Team;
        }

        if (!__instance.Session.MatchSettings.SoloMode)
        {
            ProfileSessionStats.RegisterVersusKill(killerIndex, playerIndex, deathType == DeathType.Team);
        }
    }
}