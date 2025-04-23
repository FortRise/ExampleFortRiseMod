using System;
using FortRise;

namespace Teuria.MoreReplay;

public sealed class MoreReplayModule : FortModule
{
    public static MoreReplayModule Instance = null!;

    internal Type[] Hookables = [
        typeof(DarkWorldRoundLogicPatch),
        typeof(DarkWorldGameOverPatch),
        typeof(QuestGameOverPatch),
        typeof(QuestRoundLogicPatch),
        typeof(LevelPatch),
    ];


    public MoreReplayModule() 
    {
        Instance = this;
    }

    public override void Load()
    {
        foreach (var hookable in Hookables)
        {
            hookable.GetMethod(nameof(IHookable.Load))!.Invoke(null, []);
        }
    }

    public override void Unload()
    {
        foreach (var hookable in Hookables)
        {
            hookable.GetMethod(nameof(IHookable.Unload))!.Invoke(null, []);
        }
    }
}