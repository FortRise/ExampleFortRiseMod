using System;
using Teuria.BaronMode.Hooks;
using Teuria.BaronMode.Interop;
using Teuria.BaronMode.Pickups;
using FortRise;
using MonoMod.ModInterop;
using Teuria.BaronMode.GameModes;

namespace Teuria.BaronMode;

internal class BaronModeModule : FortModule
{
    public static BaronModeModule Instance = null!;

    public override Type SettingsType => typeof(BaronModeSettings);
    public BaronModeSettings Settings => (BaronModeSettings)Instance.InternalSettings;

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
        typeof(PlayerCorpseHooks)
    ];

    public BaronModeModule() 
    {
        Instance = this;
    }

    public override void LoadContent() {}

    public override void Load()
    {
        foreach (var register in Hookables)
        {
            register.GetMethod(nameof(IHookable.Load))?.Invoke(null, []);
        }
    }

    public override void Unload()
    {
        foreach (var register in Hookables)
        {
            register.GetMethod(nameof(IHookable.Unload))?.Invoke(null, []);
        }
    }

    public override void Initialize()
    {
        WiderSetApi = Interop.GetApi<IWiderSetModApi>("Teuria.WiderSetMod");
        foreach (var register in Registerables)
        {
            register.GetMethod(nameof(IRegisterable.Register))?.Invoke(null, [Registry]);
        }
    }
}