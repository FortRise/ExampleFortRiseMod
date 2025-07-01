using FortRise;
using HarmonyLib;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class FightButtonHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(FightButton), "MenuAction"),
            postfix: new HarmonyMethod(FightButton_MenuAction_Postfix)
        );
    }

    private static void FightButton_MenuAction_Postfix(FightButton __instance)
    {
        __instance.MainMenu.State = WiderSetModule.StandardSelectionEntry.MenuState;
    }
}