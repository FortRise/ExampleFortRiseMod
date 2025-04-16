using Teuria.BaronMode.GameModes;
using TowerFall;

namespace Teuria.BaronMode.Hooks;

public class PlayerCorpseHooks : IHookable
{
    public static void Load()
    {
        On.TowerFall.PlayerCorpse.CanDoPrismHit += CanDoPrismHit_patch;
    }

    public static void Unload()
    {
        On.TowerFall.PlayerCorpse.CanDoPrismHit -= CanDoPrismHit_patch;
    }

    private static bool CanDoPrismHit_patch(On.TowerFall.PlayerCorpse.orig_CanDoPrismHit orig, PlayerCorpse self, Arrow arrow)
    {
        // Do not remove the corpse as it prevents it from respawning
        if (arrow.Level.Session.RoundLogic is BaronRoundLogic) 
        {
            return false;
        }
        return orig(self, arrow);
    }
}
