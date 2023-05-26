using FortRise;
using Monocle;
using TowerFall;

namespace BartizanMod;


[Fort("com.kha.BartizanMod", "BartizanMod")]
public class BartizanModModule : FortModule
{
    public static Atlas BartizanAtlas;

    public static BartizanModModule Instance;

    public BartizanModModule() 
    {
        Instance = this;
    }

    public override void LoadContent()
    {
        BartizanAtlas = Atlas.Create("Atlas/atlas.xml", "Atlas/atlas.png", true, ContentAccess.ModContent);
    }

    public override void OnVariantsRegister(MatchVariants variants, bool noPerPlayer = false)
    {
        var info = new VariantInfo(BartizanModModule.BartizanAtlas);
        var noHeadBounce = variants.AddVariant(
            "NoHeadBounce", info with { Header = "MODS" }, VariantFlags.PerPlayer, noPerPlayer);
        var noDodgeCooldown = variants.AddVariant(
            "NoDodgeCooldowns", info, VariantFlags.PerPlayer, noPerPlayer);
        var awfullyFastArrows = variants.AddVariant(
            "AwfullyFastArrows", info, VariantFlags.None, noPerPlayer);
        var awfullySlowArrows = variants.AddVariant(
            "AwfullySlowArrows", info, VariantFlags.None, noPerPlayer);
        var noLedgeGrab = variants.AddVariant(
            "NoLedgeGrab", info, VariantFlags.PerPlayer, noPerPlayer);
        var infiniteArrows = variants.AddVariant(
            "InfiniteArrows", info, VariantFlags.PerPlayer, noPerPlayer);
        
        noHeadBounce.IncompatibleWith(variants.NoTimeLimit);
        noDodgeCooldown.IncompatibleWith(variants.ShowDodgeCooldown);
        awfullyFastArrows.IncompatibleWith(awfullySlowArrows);
    }

    public override void Load()
    {
        RespawnRoundLogic.Load();
        MyPlayerGhost.Load();
        MyRollcallElement.Load();
        MyVersusPlayerMatchResults.Load();
        MyPlayer.Load();
        MyArrow.Load();
    }

    public override void Unload()
    {
        RespawnRoundLogic.Unload();
        MyPlayerGhost.Unload();
        MyRollcallElement.Unload();
        MyVersusPlayerMatchResults.Unload();
        MyPlayer.Unload();
        MyArrow.Unload();
    }
}
