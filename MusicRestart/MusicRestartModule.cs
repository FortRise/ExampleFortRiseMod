using FortRise;
using Microsoft.Extensions.Logging;

namespace MusicRestart;

public sealed class MusicRestartModule : Mod
{
    public static MusicRestartModule Instance = null!;

    public static MusicRestartSettings Settings => Instance.GetSettings<MusicRestartSettings>()!;


    public MusicRestartModule(IModContent content, IModuleContext context, ILogger logger) : base(content, context, logger)
    {
        Instance = this;
        PauseMenuPatch.Load(context.Harmony);
    }

    public override ModuleSettings? CreateSettings()
    {
        return new MusicRestartSettings();
    }
}