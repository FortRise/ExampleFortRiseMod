using FortRise;
using Microsoft.Extensions.Logging;

namespace Teuria.Fortfeit;


internal class FortfeitModule : Mod
{
    public FortfeitModule(IModContent content, IModuleContext context, ILogger logger) : base(content, context, logger)
    {
        context.Harmony.PatchAll(typeof(PatchSet).Assembly);
    }
}
