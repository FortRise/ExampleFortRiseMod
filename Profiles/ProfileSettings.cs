using FortRise;
using Monocle;
using TowerFall;

namespace Teuria.Profiles;


public sealed class ProfileSettings : ModuleSettings
{
    public bool HideProfileNameOnPlayer { get; set; }
    public bool HideProfileNameOnRoundResult { get; set; }
    public bool HideProfileNameOnMatchResult { get; set; }
    public string ProfileStatsChartType { get; set; } = "Relative";

    public override void Create(ISettingsCreate settings)
    {
        if (Engine.Instance.Scene is MainMenu menu)
        {
            settings.CreateInput("CREATE PROFILE", string.Empty, (x) => 
            {
                if (string.IsNullOrWhiteSpace(x))
                {
                    return;
                }

                var profileSaveData = ProfilesModule.Instance.GetSaveData<ProfileSaveData>()!;
                profileSaveData.ProfileStats.Add(new ProfileStats() { Name = x });

                var profile = new PlayerProfile() 
                {
                    Name = x
                };

                ProfilesModule.Instance.Profiles.Add(profile);

                var bundle = BundleStateManager.Instance.CreateBundle();
                bundle.Set("profile", profile);
                BundleStateManager.Instance.Push(bundle);

                ProfileSessionStats.AddOne();

                menu.State = ProfilesModule.Instance.ManageProfileState.MenuState;
            }, InputBehavior.None);
        }


        foreach (var profile in ProfilesModule.Instance.Profiles)
        {
            settings.CreateCustomOptions(() =>
            {
                var optionsButton = new QuickOptionsButton(profile.Name.ToUpperInvariant())
                {
                    OnToggle = (x) => profile.Disabled = x,
                    Disabled = profile.Disabled
                };

                optionsButton.SetCallbacks(() =>
                {
                    if (Engine.Instance.Scene is MainMenu menu)
                    {
                        var bundle = BundleStateManager.Instance.CreateBundle();
                        bundle.Set("profile", profile);
                        BundleStateManager.Instance.Push(bundle);
                        menu.State = ProfilesModule.Instance.ManageProfileState.MenuState;
                    }                   
                });

                return optionsButton;
            });
        }

        settings.CreateOnOff("HIDE PROFILE NAME ON PLAYER", HideProfileNameOnPlayer, (x) => HideProfileNameOnPlayer = x);
        settings.CreateOnOff("HIDE PROFILE NAME ON ROUND RESULT", HideProfileNameOnRoundResult, (x) => HideProfileNameOnRoundResult = x);
        settings.CreateOnOff("HIDE PROFILE NAME ON MATCH RESULT", HideProfileNameOnMatchResult, (x) => HideProfileNameOnMatchResult = x);
        settings.CreateOptions("PROFILE STATS CHART TYPE", ProfileStatsChartType, ["Relative", "Incremental"], a =>
        {
            ProfileStatsChartType = a.Item1;
        });
    }
}
