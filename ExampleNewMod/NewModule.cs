using ExampleNewMod;
using FortRise;
using Microsoft.Extensions.Logging;

namespace Teuria.NewExampleMod;

/* 
This is an example mod, please remove all of the code except Load, Unload and Initialize 
to get started.
*/

internal class NewModule : Mod
{
    public NewModule(IModContent content, IModuleContext context, ILogger logger) : base(content, context, logger)
    {
        TriggerBrambleArrow.Register(context.Harmony, content, context.Registry);
        PinkSlime.Register(context.Harmony, content, context.Registry);
    }

    public override object? GetApi()
    {
        return new ApiImplementation();
    }
}

