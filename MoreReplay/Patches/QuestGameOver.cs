using System;
using Monocle;
using MonoMod.Utils;
using TowerFall;

namespace MoreReplay;

public static class QuestGameOverPatch
{
    public static void Load() 
    {
        On.TowerFall.QuestGameOver.ctor += ctor_patch;
    }

    public static void Unload() 
    {
        On.TowerFall.QuestGameOver.ctor -= ctor_patch;
    }

    private static void ctor_patch(On.TowerFall.QuestGameOver.orig_ctor orig, QuestGameOver self, QuestRoundLogic quest)
    {
        orig(self, quest);
        var dynSelf = DynamicData.For(self);
        dynSelf.Set("replaySaved", false);
        dynSelf.Set("saving", false);

        var replayMenuComponent = new ReplayMenuComponent();
        self.Add(replayMenuComponent);
    }
}