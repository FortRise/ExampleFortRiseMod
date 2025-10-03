using System;
using Teuria.BaronMode.Hooks;
using Teuria.BaronMode.Pickups;
using FortRise;
using Teuria.BaronMode.GameModes;
using Microsoft.Extensions.Logging;
using Teuria.WiderSet;

namespace Teuria.BaronMode;

internal class BaronModeModule : Mod
{
    public static BaronModeModule Instance = null!;

    public BaronModeSettings Settings => Instance.GetSettings<BaronModeSettings>()!;

    public IWiderSetModApi? WiderSetApi { get; private set; }

    private static Type[] Registerables = [
        typeof(GemLives),
        typeof(BaronMatchVariants),
        typeof(Baron)
    ];

    private static Type[] Hookables = [
        typeof(TreasureSpawnerHooks),
        typeof(SessionHooks),
        typeof(RoundLogicHooks),
        typeof(PlayerCorpseHooks),
        typeof(VersusRoundResultsHooks)
    ];

    public override ModuleSettings? CreateSettings()
    {
        return new BaronModeSettings();
    }

    public BaronModeModule(IModContent content, IModuleContext context, ILogger logger) : base(content, context, logger)
    {
        Instance = this;

        foreach (var register in Hookables)
        {
            register.GetMethod(nameof(IHookable.Load))?.Invoke(null, [context.Harmony]);
        }

        WiderSetApi = context.Interop.GetApi<IWiderSetModApi>("Teuria.WiderSet");
        foreach (var register in Registerables)
        {
            register.GetMethod(nameof(IRegisterable.Register))?.Invoke(null, [content, context.Registry]);
        }
    }
}
