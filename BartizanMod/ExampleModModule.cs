using FortRise;
using Monocle;

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
        BartizanAtlas = Atlas.Create(ToContentPath("Atlas/atlas.xml"), ToContentPath("Atlas/atlas.png"), true);
    }

    public override void Load()
    {
        RespawnRoundLogic.Load();
        MyPlayerGhost.Load();
        MyRollcallElement.Load();
        MyVersusPlayerMatchResults.Load();
        MyMatchVariants.Load();
        MyPlayer.Load();
        MyArrow.Load();
    }

    public override void Unload()
    {
        RespawnRoundLogic.Unload();
        MyPlayerGhost.Unload();
        MyRollcallElement.Unload();
        MyVersusPlayerMatchResults.Unload();
        MyMatchVariants.Unload();
        MyPlayer.Unload();
        MyArrow.Unload();
    }
}
