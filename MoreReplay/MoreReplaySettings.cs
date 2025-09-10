using FortRise;

namespace Teuria.MoreReplay;

public class MoreReplaySettings : ModuleSettings
{
    public int FrameCount { get; set; } = 210;

    public override void Create(ISettingsCreate settings)
    {
        settings.CreateNumber("Frame Count", FrameCount, (x) => FrameCount = x, 10, 3000, 10);
    }
}
