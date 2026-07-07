using System.Collections.Generic;
using FortRise;

namespace Teuria.Profiles;

public sealed class ProfileSaveData : ModuleSaveData
{
    public List<ProfileStats> ProfileStats { get; set; } = [];

    public override void OnVerify()
    {
        base.OnVerify();

        foreach (var profile in ProfilesModule.Instance.Profiles)
        {
            bool containsName = false;
            foreach (var stat in ProfileStats)
            {
                if (stat.Name == profile.Name)
                {
                    containsName = true;
                    break;
                }
            }

            if (containsName)
            {
                continue;
            }

            ProfileStats.Add(new ProfileStats()
            {
                Name = profile.Name
            });
        }
    }
}
