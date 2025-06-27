using FortRise;

namespace MusicRestart;

public sealed class MusicRestartSettings : ModuleSettings 
{
    public bool Active { get; set; } = true;

    public override void Create(ISettingsCreate settings)
    {
        settings.CreateOnOff("Active", Active, (x) => Active = x);
    }
}