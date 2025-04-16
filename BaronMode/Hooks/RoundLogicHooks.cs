
using Teuria.BaronMode.GameModes;
using TowerFall;

namespace Teuria.BaronMode.Hooks;

public class RoundLogicHooks : IHookable
{
    public static void Load()
    {
        On.TowerFall.RoundLogic.FFACheckForAllButOneDead += FFACheckForAllButOneDead_patch;
    }

    public static void Unload()
    {
        On.TowerFall.RoundLogic.FFACheckForAllButOneDead -= FFACheckForAllButOneDead_patch;
    }

    private static bool FFACheckForAllButOneDead_patch(On.TowerFall.RoundLogic.orig_FFACheckForAllButOneDead orig, RoundLogic self)
    {
        if (self is BaronRoundLogic)
            return false;
        return orig(self);
    }
}