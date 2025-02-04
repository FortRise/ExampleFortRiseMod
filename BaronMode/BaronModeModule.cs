using System;
using BaronMode.GameModes;
using BaronMode.Hooks;
using BaronMode.Interop;
using FortRise;
using MonoMod.ModInterop;
using TowerFall;

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
        TreasureSpawnerHooks.Load();
    }

    public override void Unload()
    {
        BaronRoundLogic.Unload();
        TreasureSpawnerHooks.Unload();
    }

    public override void Initialize()
    {
        EightPlayerMod = IsModExists("WiderSetMod");
    }

    public override void OnVariantsRegister(VariantManager manager, bool noPerPlayer = false)
    {
        var treasureLivesInfo = new CustomVariantInfo(
            "TreasureLives", 
            TFGame.MenuAtlas["BaronMode/variants/treasureLives"],
            "Spawns a treasure that adds a lives by one".ToUpperInvariant(),
            CustomVariantFlags.None);
        manager.AddVariant(treasureLivesInfo);
    }
}