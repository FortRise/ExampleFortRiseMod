using System;
using System.Linq;
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

    public bool FollowsDefaultKeyboardConfig { get; set; } = true;

    [JsonIgnore]
    public int ArcherIndex
    {
        get
        {
            var entry = ProfilesModule.Instance.Context.Registry.Archers.RegisteredArchers
                .Select(x => x.Value)
                .FirstOrDefault(x => x.Name == ArcherID);

            if (entry is not null)
            {
                return entry.Index;
            }

            switch (ArcherTypes)
            {
                case ArcherData.ArcherTypes.Normal:
                    var normalData = ArcherData.Archers
                        .Where(x => x is not null)
                        .FirstOrDefault(x => x.Name0 + x.Name1 == ArcherID);
                    return Array.IndexOf(ArcherData.Archers, normalData);

                case ArcherData.ArcherTypes.Alt:
                    var altData = ArcherData.AltArchers
                        .Where(x => x is not null)
                        .FirstOrDefault(x => x.Name0 + x.Name1 == ArcherID);
                    return Array.IndexOf(ArcherData.AltArchers, altData);                   

                case ArcherData.ArcherTypes.Secret:
                    var secretData = ArcherData.SecretArchers
                        .Where(x => x is not null)
                        .FirstOrDefault(x => x.Name0 + x.Name1 == ArcherID);
                    return Array.IndexOf(ArcherData.SecretArchers, secretData);

                default:
                    return -1;
            }
        }
    }
}