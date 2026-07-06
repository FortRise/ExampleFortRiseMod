using System;
using System.Linq;
using System.Text.Json.Serialization;
using TowerFall;

namespace Teuria.Profiles;

public record ProfileArcher(string ArcherID, ArcherData.ArcherTypes ArcherType)
{
    [JsonIgnore]
    public int CharacterIndex
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

            switch (ArcherType)
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
