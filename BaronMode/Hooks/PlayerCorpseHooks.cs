using FortRise;
using HarmonyLib;
using Teuria.BaronMode.GameModes;
using TowerFall;

namespace Teuria.BaronMode.Hooks;

public class PlayerCorpseHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(PlayerCorpse), "CanDoPrismHit"),
            new HarmonyMethod(PlayerCorpse_CanDoPrismHit_Prefix)
        );
    }

    private static bool PlayerCorpse_CanDoPrismHit_Prefix(Arrow arrow, ref bool __result)
    {
        // Do not remove the corpse as it prevents it from respawning
        if (arrow.Level.Session.RoundLogic is BaronRoundLogic)
        {
            __result = false;
            return false;
        }

        return true;
    }
}
