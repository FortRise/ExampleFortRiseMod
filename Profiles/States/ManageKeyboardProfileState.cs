using System.Collections.Generic;
using FortRise;
using Microsoft.Xna.Framework;
using TowerFall;

namespace Teuria.Profiles;

public sealed class ManageKeyboardProfileState : CustomMenuState
{
    public ManageKeyboardProfileState(MainMenu main) : base(main)
    {
    }

    public override void Create()
    {
        var archerBundle = BundleStateManager.Instance.Pop();
        var profileConstruct = archerBundle.Get<PlayerProfile>("profile");

        var keyboardconfig = profileConstruct.KeyboardConfig;
        var buttons = new List<OptionsButton>();

        var jumpButton = new KeyboardInputOptionsButton("JUMP", keyboardconfig.Jump, (x) =>
        {
            keyboardconfig.Jump = x;
        });
        buttons.Add(jumpButton);

        var shootButton = new KeyboardInputOptionsButton("SHOOT", keyboardconfig.Shoot, (x) =>
        {
            keyboardconfig.Shoot = x;
        });
        buttons.Add(shootButton);

        var arrowsButton = new KeyboardInputOptionsButton("ARROWS SWAP", keyboardconfig.Arrows, (x) =>
        {
            keyboardconfig.Arrows = x;
        });
        buttons.Add(arrowsButton);

        var altShootButton = new KeyboardInputOptionsButton("ALT SHOOT", keyboardconfig.AltShoot, (x) =>
        {
            keyboardconfig.AltShoot = x;
        });
        buttons.Add(altShootButton);

        var dodgeButton = new KeyboardInputOptionsButton("DODGE", keyboardconfig.Dodge, (x) =>
        {
            keyboardconfig.Dodge = x;
        });
        buttons.Add(dodgeButton);

        var altDodgeButton = new KeyboardInputOptionsButton("ALT DODGE", keyboardconfig.MenuAlt, (x) =>
        {
            keyboardconfig.MenuAlt = x;
        });
        buttons.Add(altDodgeButton);

        var startButton = new KeyboardInputOptionsButton("START", keyboardconfig.Start, (x) =>
        {
            keyboardconfig.Start = x;
        });
        buttons.Add(startButton);


        OptionsButton resetButton = new OptionsButton("RESET ALL BUTTONS");
        resetButton.SetCallbacks(() => resetButton.State = string.Empty, null, null, () =>
        {
            keyboardconfig = KeyboardConfig.GetDefault();
            jumpButton.Buttons = keyboardconfig.Jump;
            shootButton.Buttons = keyboardconfig.Shoot;
            altShootButton.Buttons = keyboardconfig.AltShoot;
            arrowsButton.Buttons = keyboardconfig.Arrows;
            dodgeButton.Buttons = keyboardconfig.Dodge;
            altDodgeButton.Buttons = keyboardconfig.MenuAlt;
            startButton.Buttons = keyboardconfig.Start;
            return true;
        });

        buttons.Add(resetButton);
        Main.Add(buttons);

        Main.ToStartSelected = jumpButton;
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
