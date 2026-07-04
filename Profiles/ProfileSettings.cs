using FortRise;
using Monocle;
using TowerFall;

namespace Teuria.Profiles;

public sealed class ProfileSettings : ModuleSettings
{
    public override void Create(ISettingsCreate settings)
    {
        if (Engine.Instance.Scene is MainMenu menu)
        {
            settings.CreateButton("CREATE PROFILE", () =>
            {
                var bundle = BundleStateManager.Instance.CreateBundle();
                bundle.Set("state", ProfileSelectState.Create);
                BundleStateManager.Instance.Push(bundle);
                menu.State = ProfilesModule.Instance.ManageProfileState.MenuState;
            });
        }


        foreach (var profile in ProfilesModule.Instance.Profiles)
        {
            settings.CreateButton(profile.Name.ToUpperInvariant(), () =>
            {
                if (Engine.Instance.Scene is MainMenu menu)
                {
                    var bundle = BundleStateManager.Instance.CreateBundle();
                    bundle.Set("state", ProfileSelectState.Edit);
                    bundle.Set("profile", profile);
                    BundleStateManager.Instance.Push(bundle);
                    menu.State = ProfilesModule.Instance.ManageProfileState.MenuState;
                }
            });
        }
    }
}
