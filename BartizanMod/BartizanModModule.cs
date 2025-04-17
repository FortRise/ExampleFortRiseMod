using System;
using FortRise;

namespace BartizanMod;


public class BartizanModModule : FortModule
{
    public static BartizanModModule Instance = null!;
    public IWiderSetModApi? WiderSetApi { get; private set; } = null!;

    public override Type SettingsType => typeof(BartizanModSettings);
    public BartizanModSettings Settings => (BartizanModSettings)Instance.InternalSettings;

    public static bool EightPlayerMod;

    public BartizanModModule() 
    {
        Instance = this;
    }

    public override void Load()
    {
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
        WiderSetApi = Interop.GetApi<IWiderSetModApi>("Teuria.WiderSetMod");
        MyPlayer.Register(Registry);
        MyArrow.Register(Registry);

        Crawl.Register(Registry);
        Respawn.Register(Registry);
    }
}