using FortRise;
using HarmonyLib;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class MainMenuHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(MainMenu), "CreateMain"),
            new HarmonyMethod(MainMenu_CreateMain_Prefix)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(MainMenu), nameof(MainMenu.CreateRollcall)),
            transpiler: new HarmonyMethod(PlayerAmountUtilities.EightPlayerTranspiler)
        );
    }

    private static void MainMenu_CreateMain_Prefix()
    {
        WiderSetModule.IsWide = false;
    }

}
