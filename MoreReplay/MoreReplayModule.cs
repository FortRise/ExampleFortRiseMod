using System;
using FortRise;
using Microsoft.Extensions.Logging;

namespace Teuria.MoreReplay;

public sealed class MoreReplayModule : Mod
{
    public static MoreReplayModule Instance = null!;

    internal Type[] Hookables = [
        typeof(DarkWorldRoundLogicPatch),
        typeof(DarkWorldGameOverPatch),
        typeof(QuestGameOverPatch),
        typeof(QuestRoundLogicPatch),
        typeof(LevelPatch),
    ];

    public MoreReplayModule(IModContent content, IModuleContext context, ILogger logger) : base(content, context, logger)
    {
        Instance = this;

        foreach (var hookable in Hookables)
        {
            hookable.GetMethod(nameof(IHookable.Load))!.Invoke(null, [context.Harmony]);
        }
    }
}