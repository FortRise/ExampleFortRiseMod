using FortRise;
using Microsoft.Extensions.Logging;

namespace Teuria.QuestRandomizer;

public sealed class QuestRandomizerModule : Mod
{
    public static QuestRandomizerModule Instance { get; private set; } = null!;

    public QuestRandomizerModule(IModContent content, IModuleContext context, ILogger logger) : base(content, context, logger)
    {
        Instance = this;

        context.Harmony.PatchAll(typeof(QuestRandomizerModule).Assembly);
    }
}
