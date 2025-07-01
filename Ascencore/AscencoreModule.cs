using FortRise;
using Microsoft.Extensions.Logging;

namespace Teuria.Ascencore;

public sealed class AscencoreModule : Mod
{
    public static AscencoreModule Instance { get; private set; } = null!;

    public AscencoreModule(IModContent content, IModuleContext context, ILogger logger) : base(content, context, logger)
    {
        Instance = this;
    }

    public override object? GetApi() => new ApiImplementation();
}
