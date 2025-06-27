
using FortRise;
using HarmonyLib;
using Teuria.BaronMode.GameModes;
using TowerFall;

namespace Teuria.BaronMode.Hooks;

public class SessionHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Session), nameof(Session.GetWinner)),
            new HarmonyMethod(Session_GetWinner_Prefix)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Session), nameof(Session.ShouldSpawn)),
            new HarmonyMethod(Session_ShouldSpawn_Prefix)
        );
    }

    private static bool Session_ShouldSpawn_Prefix(Session __instance, int playerIndex, ref bool __result)
    {
        if (__instance.RoundLogic is BaronRoundLogic logic)
        {
            if (logic.TotalLives[playerIndex] > -1)
            {
                __result = true;
                return false;
            }

            __result = false;
            return false;
        }

        return true;
    }

    private static bool Session_GetWinner_Prefix(Session __instance, ref int __result)
    {
        if (__instance.RoundLogic is BaronRoundLogic logic)
        {
            int alive = 0;
            int lastAlive = 0;
            for (int i = 0; i < logic.TotalLives.Length; i++)
            {
                if (logic.TotalLives[i] <= -1)
                {
                    continue;
                }

                alive++;
                lastAlive = i;
            }
            if (alive == 1)
            {
                __result = lastAlive;
                return false;
            }
            if (alive == 0)
            {
                for (int i = 0; i < logic.TotalLives.Length; i++)
                {
                    foreach (Monocle.Entity entity in __instance.CurrentLevel[Monocle.GameTags.Player])
                    {
                        Player player2 = (Player)entity;
                        if (!player2.Dead)
                        {
                            __result = player2.PlayerIndex;
                            return false;
                        }
                    }
                }
            }
            __result = -1;
            return false;
        }

        return true;
    }
}
