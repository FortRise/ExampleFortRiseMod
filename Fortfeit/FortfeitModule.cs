using FortRise;
using Microsoft.Extensions.Logging;
using Teuria.WiderSet;

namespace Teuria.Fortfeit;


internal class FortfeitModule : Mod
{
    public static IWiderSetModApi? WiderSetModApi { get; set; }

    public FortfeitModule(IModContent content, IModuleContext context, ILogger logger) : base(content, context, logger)
    {
        context.Harmony.PatchAll(typeof(PatchSet).Assembly);
        WiderSetModApi = context.Interop.GetApi<IWiderSetModApi>("Teuria.WiderSet");
    }
}
