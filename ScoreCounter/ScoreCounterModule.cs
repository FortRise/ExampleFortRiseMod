using System;
using FortRise;
using Microsoft.Extensions.Logging;
using Monocle;

namespace Teuria.ScoreCounter;

public sealed class ScoreCounterModule : Mod
{
    public static ScoreCounterModule Instance { get; private set; } = null!;
    internal Type[] Hookables = [
        typeof(QuestCompleteHooks),
        typeof(QuestLevelSelectOverlayHooks),
        typeof(QuestRoundLogicHooks),
        typeof(QuestPlayerHUDHooks)
    ];

    public static ScoreCounterSaveData SaveData => Instance.GetSaveData<ScoreCounterSaveData>()!;

    public ScoreData ScoreData { get; private set; }

    public ScoreCounterModule(IModContent content, IModuleContext context, ILogger logger) : base(content, context, logger)
    {
        Instance = this;
        ScoreData = new ScoreData();

        foreach (var hookable in Hookables)
        {
            hookable.GetMethod(nameof(IHookable.Load))!.Invoke(null, [context.Harmony]);
        }

        OnInitialize += _ => Cache.Init<QuestScorePopup>();
    }

    public override ModuleSaveData CreateSaveData()
    {
        return new ScoreCounterSaveData();
    }
}
