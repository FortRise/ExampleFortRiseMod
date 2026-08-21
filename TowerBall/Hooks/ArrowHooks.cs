using HarmonyLib;
using Monocle;
using TowerFall;

namespace TowerBall;

[HarmonyPatch(typeof(Arrow))]
public static class ArrowHooks 
{
    [HarmonyPatch(nameof(Arrow.Create))]
    [HarmonyPostfix]
    public static void Create_Postfix(LevelEntity owner, Arrow __result)
    {
        if (__result is BasketBall basketBall)
        {
            basketBall.slamming = new Counter(15);
            basketBall.JustSpawned();
            basketBall.AssistIndex = ((TowerBallRoundLogic)owner.Level.Session.RoundLogic).LastThrower;
            if (owner is Player player)
            {
                basketBall.allyOop = PlayerHooks.touchedGroundSinceCollect[player.PlayerIndex];
            }
        }
    }
}


