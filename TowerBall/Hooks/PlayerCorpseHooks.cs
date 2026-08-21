using System;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace TowerBall;

[HarmonyPatch(typeof(PlayerCorpse))]
public static class PlayerCorpseHooks
{
    private static Counter removeCounter;

    [HarmonyPatch(nameof(PlayerCorpse.Added))]
    [HarmonyPostfix]
    public static void Added_Postfix(PlayerCorpse __instance)
    {
        try 
        {
            if (__instance.Level.Session.MatchSettings.Mode != TowerBall.TowerBallEntry.Modes)
            {
                return;
            }

            removeCounter = new Counter(150);
        }
        catch (Exception ex)
        {
            TowerBallModModule.Instance.Logger.LogError("An exception occurs: {ex}", ex);
        }
    }

    [HarmonyPatch(nameof(PlayerCorpse.Update))]
    [HarmonyPostfix]
    public static void Update_Postfix(PlayerCorpse __instance)
    {
        try 
        {
            if (__instance.Level.Session.MatchSettings.Mode != TowerBall.TowerBallEntry.Modes)
            {
                return;
            }

            if (!removeCounter)
            {
                return;
            }

            removeCounter.Update();
            if (!removeCounter)
            {
                __instance.Flash(60, () => 
                {
                    __instance.ArrowCushion.ReleaseArrows(Vector2.UnitY * 2);
                    __instance.RemoveSelf();
                });
            }
        }
        catch (Exception ex)
        {
            TowerBallModModule.Instance.Logger.LogError("An exception occurs: {ex}", ex);
        }
    }
}

