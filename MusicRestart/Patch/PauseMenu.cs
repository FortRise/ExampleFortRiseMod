using FortRise;
using HarmonyLib;
using Monocle;
using MonoMod.Utils;
using TowerFall;

namespace MusicRestart;

public static class PauseMenuPatch 
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(PauseMenu), "DarkWorldRestart"),
            new HarmonyMethod(PauseMenu_ModeRestart_Prefix)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(PauseMenu), "QuestRestart"),
            new HarmonyMethod(PauseMenu_ModeRestart_Prefix)
        );
    }

    private static void UsePatch(PauseMenu self) 
    {
        var level = DynamicData.For(self).Get<Level>("level")!;
        Music.Stop();
        Music.Play(level.Session.MatchSettings.LevelSystem.Theme.Music);
    }

    private static void PauseMenu_ModeRestart_Prefix(PauseMenu __instance)
    {
        if (MusicRestartModule.Settings.Active) 
        {
            UsePatch(__instance);
        }
    }
}