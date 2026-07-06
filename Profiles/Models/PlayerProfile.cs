using System.Text.Json.Serialization;
using TowerFall;

namespace Teuria.Profiles;

public class PlayerProfile
{
    public required string Name { get; set; }
    public string? ArcherID { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ArcherData.ArcherTypes ArcherTypes { get; set; } = ArcherData.ArcherTypes.Normal;

    public GamepadConfig GamepadConfig { get; set; } = GamepadConfig.GetDefault();
    public KeyboardConfig KeyboardConfig { get; set; } = KeyboardConfig.GetDefault();

    public bool FollowsDefaultKeyboardConfig { get; set; }
}

public class PlayerProfileConstruct
{
    public string Name { get; set; } = null!;
    public string ArcherID { get; set; } = null!;
    public ArcherData.ArcherTypes ArcherTypes { get; set; } = ArcherData.ArcherTypes.Normal;

    public GamepadConfig GamepadConfig { get; set; } = null!;
    public KeyboardConfig KeyboardConfig { get; set; } = null!;

    public bool FollowsDefaultKeyboardConfig { get; set; }
}