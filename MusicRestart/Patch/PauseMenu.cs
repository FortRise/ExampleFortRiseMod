using System;
using FortRise;
using Monocle;
using MonoMod.Utils;
using TowerFall;

namespace MusicRestart;

public static class PauseMenuPatch 
{

    public static void Load() 
    {
        On.TowerFall.PauseMenu.DarkWorldRestart += DarkWorldRestart_patch;
        On.TowerFall.PauseMenu.QuestRestart += QuestRestart_patch;
    }

    public static void Unload() 
    {
        On.TowerFall.PauseMenu.DarkWorldRestart -= DarkWorldRestart_patch;
        On.TowerFall.PauseMenu.QuestRestart -= QuestRestart_patch;
    }

    private static void UsePatch(PauseMenu self) 
    {
        var level = DynamicData.For(self).Get<Level>("level");
        Music.Stop();
        Music.Play(level.Session.MatchSettings.LevelSystem.Theme.Music);
    }

    private static void DarkWorldRestart_patch(On.TowerFall.PauseMenu.orig_DarkWorldRestart orig, PauseMenu self)
    {
        if (MusicRestartModule.Settings.Active) 
        {
            UsePatch(self);
        }
        orig(self);
    }

    private static void QuestRestart_patch(On.TowerFall.PauseMenu.orig_QuestRestart orig, PauseMenu self)
    {
        if (MusicRestartModule.Settings.Active) 
        {
            UsePatch(self);
        }
        orig(self);
    }
}