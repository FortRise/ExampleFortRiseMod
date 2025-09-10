using FortRise;

namespace Teuria.AutolockModifier;

public sealed class AutolockModifierSettings : ModuleSettings
{
    public bool DisableAutoLock { get; set; }
    public int MaxAngle { get; set; } = 65;
    public int MaxDistanceInPixels { get; set; } = 36;
    public bool AllowTrials { get; set; }

    public override void Create(ISettingsCreate settings)
    {
        settings.CreateOnOff("Disable Auto Lock", DisableAutoLock, (x) => DisableAutoLock = x);
        settings.CreateNumber("Max Angle", MaxAngle, (x) => MaxAngle = x, 0, 360);
        settings.CreateNumber("Max Pixel Distance", MaxDistanceInPixels, (x) => MaxDistanceInPixels = x, 0, 420);
        settings.CreateOnOff("Allow Trials", AllowTrials, (x) => AllowTrials = x);
        settings.CreateButton("Reset to Defaults", () => 
        {
            DisableAutoLock = false;
            MaxAngle = 65;
            MaxDistanceInPixels = 36;
            AllowTrials = false;
            settings.Refresh();
        });

    }
}
