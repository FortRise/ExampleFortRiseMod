using FortRise;

namespace Teuria.BaronMode;

public class BaronModeSettings : ModuleSettings 
{
    [SettingsNumber(1, 20)]
    public int BaronLivesCount = 3;
}