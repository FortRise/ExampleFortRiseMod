using FortRise;

namespace Teuria.BaronMode;

public class BaronModeSettings : ModuleSettings 
{
    public int BaronLivesCount { get; set; } = 3;

    public override void Create(ISettingsCreate settings)
    {
        settings.CreateNumber("Baron Lives Count", BaronLivesCount, (x) => BaronLivesCount = x, 1, 20);
    }
}
