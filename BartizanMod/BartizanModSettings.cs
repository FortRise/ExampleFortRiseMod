using FortRise;

namespace BartizanMod;

public class BartizanModSettings : ModuleSettings 
{
    public string RespawnMode { get; set; } = "Instant";

    public override void Create(ISettingsCreate settings)
    {
        settings.CreateOptions("RespawnMode", "Instant", [
            "Instant", "Delayed"
        ], (x) => RespawnMode = x.Item1);
    }
}