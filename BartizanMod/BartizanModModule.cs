using System;
using System.Reflection;
using FortRise;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;
using MonoMod.ModInterop;
using MonoMod.Utils;
using TowerFall;

namespace BartizanMod;


[Fort("com.kha.BartizanMod", "BartizanMod")]
public class BartizanModModule : FortModule
{
    public static BartizanModModule Instance = null!;

    public override Type SettingsType => typeof(BartizanModSettings);
    public BartizanModSettings Settings => (BartizanModSettings)Instance.InternalSettings;

    public static bool EightPlayerMod;

    public BartizanModModule() 
    {
        Instance = this;
    }

    public override void OnVariantsRegister(VariantManager manager, bool noPerPlayer = false) 
    {
        var info = new CustomVariantInfo(
                            // You can directly use Subtexture from your atlas
            "NoHeadBounce", TFGame.MenuAtlas["Bartizan/variants/noHeadBounce"], 
            CustomVariantFlags.PerPlayer | CustomVariantFlags.CanRandom);
        var noHeadBounce = manager.AddVariant(info, noPerPlayer);
        var noDodgeCooldown = manager.AddVariant(DeclareFromInfo("NoDodgeCooldowns"), noPerPlayer);
        var awfullyFastArrows = manager.AddVariant(DeclareFromInfo("AwfullyFastArrows"), noPerPlayer);
        var awfullySlowArrows = manager.AddVariant(DeclareFromInfo("AwfullySlowArrows"), noPerPlayer);
        var noLedgeGrab = manager.AddVariant(DeclareFromInfo("NoLedgeGrab"), noPerPlayer);
        var infiniteArrows = manager.AddVariant(DeclareFromInfo("InfiniteArrows"), noPerPlayer);
        
        manager.CreateLinks(manager.MatchVariants.ShowDodgeCooldown, noDodgeCooldown);
        manager.CreateLinks(awfullyFastArrows, awfullySlowArrows);

        CustomVariantInfo DeclareFromInfo(string name) 
        {
            return info with {
                Name = name,
                Icon = TFGame.MenuAtlas["Bartizan/variants/" + char.ToLowerInvariant(name[0]) + name[1..]]
            };
        }
    }

    public override void Load()
    {
        typeof(EightPlayerImport).ModInterop();
        RespawnRoundLogic.Load();
        MyPlayerGhost.Load();
        MyRollcallElement.Load();
        MyVersusPlayerMatchResults.Load();
        MyPlayer.Load();
        MyArrow.Load();
        MyPlayerCorpse.Load();
    }

    public override void Unload()
    {
        RespawnRoundLogic.Unload();
        MyPlayerGhost.Unload();
        MyRollcallElement.Unload();
        MyVersusPlayerMatchResults.Unload();
        MyPlayer.Unload();
        MyArrow.Unload();
        MyPlayerCorpse.Unload();
    }

    public override void Initialize()
    {
        EightPlayerMod = IsModExists("WiderSetMod");
    }
}