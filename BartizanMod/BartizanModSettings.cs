using FortRise;

namespace BartizanMod;

public class BartizanModSettings : ModuleSettings 
{
    public const int Instant = 0;
    public const int Delayed = 1;
    [SettingsOptions("Instant", "Delayed")]
    public int RespawnMode;
}