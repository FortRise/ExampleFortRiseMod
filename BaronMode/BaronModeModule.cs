using System;
using BaronMode.GameModes;
using BaronMode.Interop;
using FortRise;
using MonoMod.ModInterop;

namespace BaronMode;

[Fort("com.kha.BartizanMod", "BartizanMod")]
public class BaronModeModule : FortModule
{
    public static BaronModeModule Instance;

    public override Type SettingsType => typeof(BaronModeSettings);
    public BaronModeSettings Settings => (BaronModeSettings)Instance.InternalSettings;

    public static bool EightPlayerMod;

    public BaronModeModule() 
    {
        Instance = this;
    }

    public override void LoadContent() {}

    public override void Load()
    {
        typeof(EightPlayerImport).ModInterop();
        BaronRoundLogic.Load();
    }

    public override void Unload()
    {
        BaronRoundLogic.Unload();
    }

    public override void Initialize()
    {
        EightPlayerMod = IsModExists("WiderSetMod");
    }
}