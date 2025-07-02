using FortRise;
using HarmonyLib;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class ScreenTitleHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredConstructor(typeof(ScreenTitle), [typeof(MainMenu.MenuState)]),
            postfix: new HarmonyMethod(ScreenTitle_ctor_Postfix)
        );
    }

    private static void ScreenTitle_ctor_Postfix(ScreenTitle __instance)
    {
        __instance.LayerIndex = -1;
    }
}