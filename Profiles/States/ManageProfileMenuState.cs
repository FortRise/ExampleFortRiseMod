using System;
using System.Collections.Generic;
using FortRise;
using Microsoft.Xna.Framework;
using MonoMod.Utils;
using TowerFall;

namespace Teuria.Profiles;

public sealed class ManageProfileMenuState : CustomMenuState
{
    public ManageProfileMenuState(MainMenu main) : base(main)
    {
    }

    public override void Create()
    {
        var bundle = BundleStateManager.Instance.Pop();
        var state = bundle.Get<ProfileSelectState>("state");

        PlayerProfile profile;

        if (state == ProfileSelectState.Edit || (bundle.TryGet("alreadyinside", out bool v) && v))
        {
            profile = bundle.Get<PlayerProfile>("profile");
        }
        else 
        {
            profile = new PlayerProfile
            {
                Name = string.Empty,
                GamepadConfig = GamepadConfig.GetDefault(),
                KeyboardConfig = KeyboardConfig.GetDefault()
            };
        }


        var buttons = new List<OptionsButton>();
        var nameButton = CreateInputText("PROFILE NAME", InputBehavior.None, profile.Name, (x) =>
        {
            if (state == ProfileSelectState.Edit && profile.Name != x)
            {
                ProfilesModule.Instance.Context.Storage.Delete($"Profiles/{profile.Name}.json", false);
            }
            ProfilesModule.Instance.Profiles.Remove(profile);
            profile.Name = x;
            ProfilesModule.Instance.Profiles.Add(profile);

            if (state == ProfileSelectState.Edit)
            {
                var saver = new Saver(true);
                Main.Add(saver);
            }
        });

        buttons.Add(nameButton);

        var selectArcher = new OptionsButton("ARCHER");
        selectArcher.SetCallbacks(() =>
        {
            var movingBundle = BundleStateManager.Instance.CreateBundle();
            movingBundle.Set("state", state);
            movingBundle.Set("alreadyinside", true);
            if (bundle.TryGet<PlayerProfile>("profile", out var prof))
            {
                movingBundle.Set("profile", prof);
            }
            else
            {
                movingBundle.Set("profile", profile);
            }
            BundleStateManager.Instance.Push(movingBundle);

            var archerBundle = BundleStateManager.Instance.CreateBundle();
            archerBundle.Set("profile", profile);
            BundleStateManager.Instance.Push(archerBundle);

            Main.State = ProfilesModule.Instance.SelectArcherState.MenuState;
        });
        buttons.Add(selectArcher);

        var gamepadConfig = new OptionsButton("GAMEPAD CONFIGURATION");
        gamepadConfig.SetCallbacks(() =>
        {
            var movingBundle = BundleStateManager.Instance.CreateBundle();
            movingBundle.Set("state", state);
            movingBundle.Set("alreadyinside", true);
            if (bundle.TryGet<PlayerProfile>("profile", out var prof))
            {
                movingBundle.Set("profile", prof);
            }
            else
            {
                movingBundle.Set("profile", profile);
            }
            BundleStateManager.Instance.Push(movingBundle);

            var archerBundle = BundleStateManager.Instance.CreateBundle();
            archerBundle.Set("profile", profile);
            BundleStateManager.Instance.Push(archerBundle);

            Main.State = ProfilesModule.Instance.GamepadProfileState.MenuState;
        });
        buttons.Add(gamepadConfig);

        var keyboardConfig = new OptionsButton("KEYBOARD CONFIGURATION");
        keyboardConfig.SetCallbacks(() =>
        {
            if (profile.FollowsDefaultKeyboardConfig)
            {
                var alert = new UIFollowDefaultKeyboardConfigAlert(keyboardConfig);
                Main.Add(alert);
                return;
            }

            var movingBundle = BundleStateManager.Instance.CreateBundle();
            movingBundle.Set("state", state);
            movingBundle.Set("alreadyinside", true);
            if (bundle.TryGet<PlayerProfile>("profile", out var prof))
            {
                movingBundle.Set("profile", prof);
            }
            else
            {
                movingBundle.Set("profile", profile);
            }
            BundleStateManager.Instance.Push(movingBundle);

            var archerBundle = BundleStateManager.Instance.CreateBundle();
            archerBundle.Set("profile", profile);
            BundleStateManager.Instance.Push(archerBundle);

            Main.State = ProfilesModule.Instance.KeyboardProfileState.MenuState;
        });
        buttons.Add(keyboardConfig);

        var followsDefaultKeyboardConfig = new OptionsButton("FOLLOWS DEFAULT KEYBOARD CONFIG");
        followsDefaultKeyboardConfig.SetCallbacks(() => followsDefaultKeyboardConfig.State = profile.FollowsDefaultKeyboardConfig ? "ON" : "OFF", null, null, () =>
        {
            profile.FollowsDefaultKeyboardConfig = !profile.FollowsDefaultKeyboardConfig;
            return profile.FollowsDefaultKeyboardConfig;
        });
        buttons.Add(followsDefaultKeyboardConfig);

        if (state == ProfileSelectState.Edit)
        {
            var dangerZone = new OptionsButtonHeader("DANGER ZONE");
            buttons.Add(dangerZone);

            var disableButton = new OptionsButton("DISABLE PROFILE");
            disableButton.SetCallbacks(() => disableButton.State = bundle.Get<PlayerProfile>("profile").Disabled ? "ON" : "OFF", null, null, () =>
            {
                var profile = bundle.Get<PlayerProfile>("profile");
                profile.Disabled = !profile.Disabled;
                return profile.Disabled;
            });

            buttons.Add(disableButton);

            var deleteButton = new OptionsButton("DELETE PROFILE");
            deleteButton.SetCallbacks(() =>
            {
                var profile = bundle.Get<PlayerProfile>("profile");

                ProfilesModule.Instance.Profiles.Remove(profile);
                Main.State = MainMenu.MenuState.Options;

                ProfilesModule.Instance.Context.Storage.Delete($"Profiles/{profile.Name}.json", false);
            });

            buttons.Add(deleteButton);
        }
        else
        {
            var createButton = new OptionsButton("CREATE");
            createButton.SetCallbacks(() =>
            {
                if (string.IsNullOrEmpty(profile.Name))
                {
                    ShowAlert(createButton, "Name field is required");
                    return;
                }
                
                ProfilesModule.Instance.Profiles.Add(profile);
                Main.State = MainMenu.MenuState.Options;
            });

            buttons.Add(createButton);
        }

        InitOptions(buttons, out int offset);

        Main.Add(buttons);

        Main.MaxUICameraY = offset;
        Main.BackState = MainMenu.MenuState.Options;
        Main.ToStartSelected = nameButton;
    }

    public override void Destroy()
    {
    }

    private OptionsButton CreateInputText(string title, InputBehavior inputBehavior, string initialValue, Action<string> onInput)
    {
        var optionButtons = new OptionsButton(title);

        // HACK: Option button does not have a value.
        var dynSelf = DynamicData.For(optionButtons);
        initialValue ??= "";
        dynSelf.Set("value", initialValue); 
        
        optionButtons.SetCallbacks(() =>
        {
            if (inputBehavior == InputBehavior.Sensitive)
            {
                optionButtons.State = "***";
            }
            else
            {
                string value = dynSelf.Get<string>("value")!;
                optionButtons.State = value.ToUpperInvariant();
            }
        },
        null, null,
        () =>
        {
            string val = dynSelf.Get<string>("value")!;
            var uiInput = new UIInputText(optionButtons, (x) =>
            {
                onInput(x);
                dynSelf.Set("value", x);
                optionButtons.Selected = true;
                optionButtons.State = x.ToUpperInvariant();
            }, new Vector2(0, 240 * 0.5f), -1, val)
            {
                LayerIndex = 0
            };

            Main.Add(uiInput);
            optionButtons.Selected = false;
            return true;
        });

        return optionButtons;
    }

    private static void InitOptions(List<OptionsButton> buttons, out int offset)
    {
        int num = 0;
        int extraSpacing = 0;
        for (int i = 0; i < buttons.Count; i++)
        {
            OptionsButton optionsButton = buttons[i];
            optionsButton.TweenTo = new Vector2(200f, 45 + extraSpacing + i * 12);
            optionsButton.Position = optionsButton.TweenFrom = new Vector2((i % 2 == 0) ? (-160) : 480, 45 + extraSpacing + i * 12);

            if (optionsButton is not OptionsButtonHeader)
            {
                int i2 = 1;
                if (i > 0)
                {
                    var button = buttons[i - 1];
                    while (button is OptionsButtonHeader)
                    {
                        i2 += 1;
                        if (i - i2 < 0)
                        {
                            break;
                        }
                        button = buttons[i - i2];
                    }

                    optionsButton.UpItem = button;
                }
                i2 = 1;

                if (i < buttons.Count - 1)
                {
                    var button = buttons[i + i2];
                    while (button is OptionsButtonHeader)
                    {
                        i2 += 1;
                        if (i - i2 > buttons.Count)
                        {
                            break;
                        }
                        button = buttons[i + i2];
                        extraSpacing += 6;
                    }

                    optionsButton.DownItem = button;
                }
            }

            num += 9 + extraSpacing;
        }

        offset = num;
    }

    private void ShowAlert(OptionsButton responsibleButton, string text)
    {
        responsibleButton.Selected = false;
        Main.CanAct = false;
        UIModal modal = new UIModal();
        modal.AddFiller(text);
        modal.AddItem("OK", () =>
        {
            responsibleButton.Selected = true;
            Main.CanAct = true;
        });
        modal.SetOnBackCallBack(() =>
        {
            responsibleButton.Selected = true;
            Main.CanAct = true;
        });

        Main.Add(modal);
    }
}
