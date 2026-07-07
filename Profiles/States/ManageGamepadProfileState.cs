using System;
using System.Collections.Generic;
using FortRise;
using Microsoft.Xna.Framework;
using TowerFall;

namespace Teuria.Profiles;

public sealed class ManageGamepadProfileState : CustomMenuState
{
    public ManageGamepadProfileState(MainMenu main) : base(main)
    {
    }

    public override void Create()
    {
        var archerBundle = BundleStateManager.Instance.Pop();
        var profileConstruct = archerBundle.Get<PlayerProfile>("profile");

        var gamepadConfig = profileConstruct.GamepadConfig;
        var buttons = new List<OptionsButton>();

        // TODO: We'll look back into this

        // OptionsButton buttonSetButton = new OptionsButton("BUTTON SET");
        // buttonSetButton.SetCallbacks(() => buttonSetButton.State = Config.ButtonSet?.ToUpperInvariant() ?? "AUTOMATIC", null, null, () =>
        // {
        //     var index = XGamepadInput.ButtonSets.IndexOf(Config.ButtonSet);
        //     if (index == -1)
        //     {
        //         xGamepadInput.ChangeButtonSet(XGamepadInput.ButtonSets[0]);
        //     }
        //     else if (index == XGamepadInput.ButtonSets.Length - 1)
        //     {
        //         xGamepadInput.ChangeButtonSet("Automatic");
        //     }
        //     else
        //     {
        //         xGamepadInput.ChangeButtonSet(XGamepadInput.ButtonSets[index + 1]);
        //     }

        //     MenuButtons.Update();
        //     return true;
        // });
        // buttons.Add(buttonSetButton);

        OptionsButton moveXDeadzone = new OptionsButton("MOVE X DEADZONE");
        moveXDeadzone.SetCallbacks(
            () =>
            {
                moveXDeadzone.State = $"{Math.Round(gamepadConfig.MoveXDeadzone * 100)}";
                moveXDeadzone.CanLeft = gamepadConfig.MoveXDeadzone > 0;
                moveXDeadzone.CanRight = gamepadConfig.MoveXDeadzone < 1f;
            }, 
            () => gamepadConfig.MoveXDeadzone = (float)(((decimal)gamepadConfig.MoveXDeadzone) - 0.1M), 
            () => gamepadConfig.MoveXDeadzone = (float)(((decimal)gamepadConfig.MoveXDeadzone) + 0.1M), 
            () => false
        );
        buttons.Add(moveXDeadzone);

        OptionsButton moveYDeadzone = new OptionsButton("MOVE Y DEADZONE");
        moveYDeadzone.SetCallbacks(
            () =>
            {
                moveYDeadzone.State = $"{Math.Round(gamepadConfig.MoveYDeadzone * 100)}";
                moveYDeadzone.CanLeft = gamepadConfig.MoveYDeadzone > 0;
                moveYDeadzone.CanRight = gamepadConfig.MoveYDeadzone < 1f;
            }, 
            () => gamepadConfig.MoveYDeadzone = (float)(((decimal)gamepadConfig.MoveYDeadzone) - 0.1M), 
            () => gamepadConfig.MoveYDeadzone = (float)(((decimal)gamepadConfig.MoveYDeadzone) + 0.1M), 
            () => false
        );
        buttons.Add(moveYDeadzone);

        var jumpButton = new GamepadInputOptionsButton("JUMP", gamepadConfig.Jump, (x) =>
        {
            gamepadConfig.Jump = x;
        });
        buttons.Add(jumpButton);

        var shootButton = new GamepadInputOptionsButton("SHOOT", gamepadConfig.Shoot, (x) =>
        {
            gamepadConfig.Shoot = x;
        });
        buttons.Add(shootButton);

        var arrowsButton = new GamepadInputOptionsButton("ARROWS SWAP", gamepadConfig.Arrows, (x) =>
        {
            gamepadConfig.Arrows = x;
        });
        buttons.Add(arrowsButton);

        var altShootButton = new GamepadInputOptionsButton("ALT SHOOT", gamepadConfig.AltShoot, (x) =>
        {
            gamepadConfig.AltShoot = x;
        });
        buttons.Add(altShootButton);

        var dodgeButton = new GamepadInputOptionsButton("DODGE", gamepadConfig.Dodge, (x) =>
        {
            gamepadConfig.Dodge = x;
        });
        buttons.Add(dodgeButton);

        var altDodgeButton = new GamepadInputOptionsButton("ALT DODGE", gamepadConfig.MenuAlt, (x) =>
        {
            gamepadConfig.MenuAlt = x;
        });
        buttons.Add(altDodgeButton);

        var startButton = new GamepadInputOptionsButton("START", gamepadConfig.Start, (x) =>
        {
            gamepadConfig.Start = x;
        });
        buttons.Add(startButton);


        OptionsButton resetButton = new OptionsButton("RESET ALL BUTTONS");
        resetButton.SetCallbacks(() => resetButton.State = string.Empty, null, null, () =>
        {
            gamepadConfig = GamepadConfig.GetDefault();
            jumpButton.Buttons = gamepadConfig.Jump;
            shootButton.Buttons = gamepadConfig.Shoot;
            altShootButton.Buttons = gamepadConfig.AltShoot;
            arrowsButton.Buttons = gamepadConfig.Arrows;
            dodgeButton.Buttons = gamepadConfig.Dodge;
            altDodgeButton.Buttons = gamepadConfig.MenuAlt;
            startButton.Buttons = gamepadConfig.Start;
            return true;
        });

        buttons.Add(resetButton);
        Main.Add(buttons);

        Main.ToStartSelected = moveXDeadzone;
        Main.BackState = ProfilesModule.Instance.ManageProfileState.MenuState;

        InitOptions(buttons, out int offset);
        Main.MaxUICameraY = offset;
    }

    public override void Destroy()
    {
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
}
