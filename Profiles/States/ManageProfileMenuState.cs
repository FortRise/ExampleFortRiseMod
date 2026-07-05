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

        PlayerProfileConstruct construct = new PlayerProfileConstruct()
        {
            Name = string.Empty
        };

        if (state == ProfileSelectState.Edit)
        {
            var profile = bundle.Get<PlayerProfile>("profile");
            construct.Name = profile.Name;
            if (profile.ArcherID != null)
            {
                construct.ArcherID = profile.ArcherID;
            }
            construct.ArcherTypes = profile.ArcherTypes;
            construct.GamepadConfig = profile.GamepadConfig ?? GamepadConfig.GetDefault();
            construct.KeyboardConfig = profile.KeyboardConfig ?? KeyboardConfig.GetDefault();
        }
        else
        {
            construct.GamepadConfig = GamepadConfig.GetDefault();
            construct.KeyboardConfig = KeyboardConfig.GetDefault();
        }

        if (bundle.TryGet("construct", out PlayerProfileConstruct? newConstr))
        {
            construct = newConstr;
        }

        var buttons = new List<OptionsButton>();
        var nameButton = CreateInputText("PROFILE NAME", InputBehavior.None, construct.Name, (x) =>
        {
            construct.Name = x;
        });

        buttons.Add(nameButton);

        var selectArcher = new ArcherOptionsButton("ARCHER", construct.ArcherID);
        selectArcher.SetCallbacks(() =>
        {
            var movingBundle = BundleStateManager.Instance.CreateBundle();
            movingBundle.Set("state", state);
            if (state == ProfileSelectState.Edit)
            {
                var profile = bundle.Get<PlayerProfile>("profile");
                movingBundle.Set("profile", profile);
            }
            BundleStateManager.Instance.Push(movingBundle);

            var archerBundle = BundleStateManager.Instance.CreateBundle();
            archerBundle.Set("construct", construct);
            BundleStateManager.Instance.Push(archerBundle);

            Main.State = ProfilesModule.Instance.SelectArcherState.MenuState;
        });
        buttons.Add(selectArcher);

        var gamepadConfig = new OptionsButton("GAMEPAD CONFIGURATION");
        gamepadConfig.SetCallbacks(() =>
        {
            var movingBundle = BundleStateManager.Instance.CreateBundle();
            movingBundle.Set("state", state);
            if (state == ProfileSelectState.Edit)
            {
                var profile = bundle.Get<PlayerProfile>("profile");
                movingBundle.Set("profile", profile);
            }
            BundleStateManager.Instance.Push(movingBundle);

            var archerBundle = BundleStateManager.Instance.CreateBundle();
            archerBundle.Set("construct", construct);
            BundleStateManager.Instance.Push(archerBundle);

            Main.State = ProfilesModule.Instance.GamepadProfileState.MenuState;
        });
        buttons.Add(gamepadConfig);

        var keyboardConfig = new OptionsButton("KEYBOARD CONFIGURATION");
        keyboardConfig.SetCallbacks(() =>
        {
            var movingBundle = BundleStateManager.Instance.CreateBundle();
            movingBundle.Set("state", state);
            if (state == ProfileSelectState.Edit)
            {
                var profile = bundle.Get<PlayerProfile>("profile");
                movingBundle.Set("profile", profile);
            }
            BundleStateManager.Instance.Push(movingBundle);

            var archerBundle = BundleStateManager.Instance.CreateBundle();
            archerBundle.Set("construct", construct);
            BundleStateManager.Instance.Push(archerBundle);

            Main.State = ProfilesModule.Instance.KeyboardProfileState.MenuState;
        });
        buttons.Add(keyboardConfig);

        if (state == ProfileSelectState.Edit)
        {
            var createButton = new OptionsButton("APPLY CHANGE");
            createButton.SetCallbacks(() =>
            {
                var profile = bundle.Get<PlayerProfile>("profile");
                if (profile.Name != construct.Name)
                {
                    ProfilesModule.Instance.Context.Storage.Delete($"Profiles/{profile.Name}.json", false);
                }

                profile.Name = construct.Name;
                profile.ArcherID = construct.ArcherID;
                profile.ArcherTypes = construct.ArcherTypes;

                Main.State = MainMenu.MenuState.Options;
            });

            buttons.Add(createButton);

            var dangerZone = new OptionsButtonHeader("DANGER ZONE");
            buttons.Add(dangerZone);

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
                if (string.IsNullOrEmpty(construct.Name))
                {
                    ShowAlert(createButton, "Name field is required");
                    return;
                }
                
                ProfilesModule.Instance.Profiles.Add(new PlayerProfile()
                {
                    Name = construct.Name,
                    ArcherID = construct.ArcherID,
                    ArcherTypes = construct.ArcherTypes,
                    GamepadConfig = construct.GamepadConfig
                });
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
