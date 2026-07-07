using System;
using FortRise;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace Teuria.Profiles;

public class QuickOptionsButton : OptionsButton
{
    public bool Disabled { get; set; }
    public Color SelectedDisableColor = Calc.HexToColor("53602A");
	public Color NotSelectedDisableColor = Calc.HexToColor("114762");
    public required Action<bool> OnToggle { get; set; }

    public QuickOptionsButton(string title) : base(title)
    {
    }

    protected override void OnSelect()
    {
        base.OnSelect();
        MainMenu.ButtonGuideA.SetDetails(MenuButtonGuide.ButtonModes.Arrows, "TOGGLE");
    }

    protected override void OnDeselect()
    {
        base.OnDeselect();
        MainMenu.ButtonGuideA.Clear();
    }

    public override void Update()
    {
        base.Update();
        if (!Selected)
        {
            return;
        }

        if (MenuInput.Arrows)
        {
            Disabled = !Disabled;
            OnToggle(Disabled);
        }
    }

    public override void Render()
    {
        base.Render();
        var changedWiggler = Private.Field<OptionsButton, Wiggler>("changedWiggler", this).Read();
        var selectedWiggler = Private.Field<OptionsButton, Wiggler>("selectedWiggler", this).Read();
        var wiggleDir = Private.Field<OptionsButton, int>("wiggleDir", this).Read();
        var title = Private.Field<OptionsButton, string>("title", this).Read();

        Vector2 pos = new Vector2(30f + 2f * changedWiggler.Value * wiggleDir, 0f);
        Color color;
        if (Disabled)
        {
            color = Selected ? SelectedDisableColor : NotSelectedDisableColor;
        }
        else
        {
            color = Selected ? SelectedColor : NotSelectedColor;
        }

        Draw.OutlineTextJustify(TFGame.Font, title, Position + new Vector2(-5f, 0f) + new Vector2(5f * selectedWiggler.Value, 0f), color, Color.Black, new Vector2(1f, 0.5f), 1f);

        if (State == "ON")
        {
            Draw.OutlineTextureCentered(TFGame.MenuAtlas["optionOn"], Position + pos, color);
        }
        else if (State == "OFF")
        {
            Draw.OutlineTextureCentered(TFGame.MenuAtlas["optionOff"], Position + pos, color);
        }
        else
        {
            Draw.OutlineTextJustify(TFGame.Font, State, Position + pos, color, Color.Black, Vector2.One * 0.5f, 1f);
        }
    }
}