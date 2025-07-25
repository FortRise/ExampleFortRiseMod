using FortRise;

namespace Teuria.AutolockModifier;

public sealed class AutolockModifierSettings : ModuleSettings
{
    public bool DisableAutoLock { get; set; } = true;
    public int MaxAngle { get; set; } = 65;

    public override void Create(ISettingsCreate settings)
    {
        settings.CreateOnOff("Disable Auto Lock", DisableAutoLock, (x) => DisableAutoLock = x);
        settings.CreateNumber("Max Angle", MaxAngle, (x) => MaxAngle = x, 0, 360);
    }
}