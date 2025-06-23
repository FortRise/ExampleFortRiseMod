using FortRise;
using Microsoft.Extensions.Logging;

namespace BartizanMod;

public class BartizanModModule : Mod
{
    public static BartizanModModule Instance = null!;
    public IWiderSetModApi? WiderSetApi { get; private set; } = null!;

    public BartizanModSettings Settings => Instance.GetSettings<BartizanModSettings>()!;
    public override ModuleSettings? CreateSettings() => new BartizanModSettings();

    public static bool EightPlayerMod;

    public BartizanModModule(IModContent content, IModuleContext context, ILogger logger) : base(content, context, logger)
    {
        Instance = this;
        WiderSetApi = context.Interop.GetApi<IWiderSetModApi>("Teuria.WiderSetMod");
        MyPlayer.Register(context.Harmony, content, context.Registry);
        MyArrow.Register(context.Harmony, content, context.Registry);
        MyPlayerCorpse.Register(context.Harmony);
        MyPlayerGhost.Register(context.Harmony);
        RespawnRoundLogic.Register(context.Harmony);
        MyRollcallElement.Register(context.Harmony);
        MyVersusPlayerMatchResults.Register(context.Harmony);

        Respawn.Register(content, context.Registry);
        Crawl.Register(content, context.Registry);
    }
}