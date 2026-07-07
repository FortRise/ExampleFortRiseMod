using System;
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

        if (killerIndex != -1 && !__instance.Session.MatchSettings.SoloMode)
        {
            var killerProfile = ProfilesModule.Instance.ProfileActive[killerIndex];
            if (killerProfile is not null)
            {
                var saveData = ProfilesModule.Instance.GetSaveData<ProfileSaveData>()!;

                foreach (var data in saveData.ProfileStats)
                {
                    if (data.Name == killerProfile.Name)
                    {
                        data.Kills += 1;
                        goto BREAKOUT;
                    }
                }

                saveData.ProfileStats.Add(new ProfileStats() { Name = killerProfile.Name, Kills = 1 });
            }
        }

    BREAKOUT:

        if (!__instance.Session.MatchSettings.SoloMode)
        {
            ProfileSessionStats.RegisterVersusKill(killerIndex, playerIndex, deathType == DeathType.Team);
        }
    }
}