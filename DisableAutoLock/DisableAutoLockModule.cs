using FortRise;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using TowerFall;

namespace Teuria.DisableAutoLock;

public sealed class DisableAutoLockModule : Mod
{
    public static DisableAutoLockModule Instance { get; private set; } = null!;

    public DisableAutoLockModule(IModContent content, IModuleContext context, ILogger logger) : base(content, context, logger)
    {
        Instance = this;
        context.Harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Player), "FindAutoLockAngle"),
            prefix: new HarmonyMethod(Player_FindAutoLockAngle_Prefix, priority: -400)
        );
    }

    private static bool Player_FindAutoLockAngle_Prefix(Player __instance, ref float __result)
    {
        DisableAutoLockSettings settings;
        if ((settings = Instance.GetSettings<DisableAutoLockSettings>()!) != null && settings.Enabled)
        {
            __result = __instance.AimDirection;
            return false;
        }
        return true;
    }


    public override ModuleSettings? CreateSettings()
    {
        return new DisableAutoLockSettings();
    }
}
