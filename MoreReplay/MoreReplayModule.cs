using System;
using FortRise;

namespace MoreReplay;

[Fort("com.terria.morereplay", "More Replay")]
public sealed class MoreReplayModule : FortModule
{
    public static MoreReplayModule Instance;


    public MoreReplayModule() 
    {
        Instance = this;
    }

    public override void Load()
    {
        DarkWorldRoundLogicPatch.Load();
        DarkWorldGameOverPatch.Load();
        QuestGameOverPatch.Load();
        QuestRoundLogicPatch.Load();
        LevelPatch.Load();
    }

    public override void Unload()
    {
        DarkWorldRoundLogicPatch.Unload();
        DarkWorldGameOverPatch.Unload();
        QuestGameOverPatch.Unload();
        QuestRoundLogicPatch.Unload();
        LevelPatch.Unload();
    }
}