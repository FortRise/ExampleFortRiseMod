using FortRise;

namespace Teuria.DisableAutoLock;

internal sealed class DisableAutoLockSettings : ModuleSettings
{
    public bool Enabled { get; set; } = true;

    public override void Create(ISettingsCreate settings)
    {
        settings.CreateOnOff("Enabled", Enabled, (x) => Enabled = x);
    }
}