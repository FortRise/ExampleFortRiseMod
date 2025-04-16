
using Teuria.BaronMode.GameModes;
using TowerFall;

namespace Teuria.BaronMode.Hooks;

public class SessionHooks : IHookable
{
    public static void Load()
    {
        On.TowerFall.Session.GetWinner += GetWinner_patch;
        On.TowerFall.Session.ShouldSpawn += ShouldSpawn_patch;
    }

    public static void Unload()
    {
        On.TowerFall.Session.GetWinner -= GetWinner_patch;
        On.TowerFall.Session.ShouldSpawn -= ShouldSpawn_patch;
    }


    private static bool ShouldSpawn_patch(On.TowerFall.Session.orig_ShouldSpawn orig, Session self, int playerIndex)
    {
        if (self.RoundLogic is BaronRoundLogic logic) 
        {
            if (logic.TotalLives[playerIndex] > -1) 
            {
                return true;
            }
            return false;
        }
        return orig(self, playerIndex);
    }

    private static int GetWinner_patch(On.TowerFall.Session.orig_GetWinner orig, Session self)
    {
        if (self.RoundLogic is BaronRoundLogic logic) 
        {
            int alive = 0;
            int lastAlive = 0;
            for (int i = 0; i < logic.TotalLives.Length; i++) 
            {
                if (logic.TotalLives[i] <= -1) 
                    continue;
                alive++;
                lastAlive = i;
            }
            if (alive == 1) 
            {
                return lastAlive;
            }
            if (alive == 0) 
            {
                for (int i = 0; i < logic.TotalLives.Length; i++) 
                {
                    foreach (Monocle.Entity entity in self.CurrentLevel[Monocle.GameTags.Player])
                    {
                        Player player2 = (Player)entity;
                        if (!player2.Dead)
                        {
                            return player2.PlayerIndex;
                        }
                    }
                }
            }
            return -1;
        }
        return orig(self);
    }
}
