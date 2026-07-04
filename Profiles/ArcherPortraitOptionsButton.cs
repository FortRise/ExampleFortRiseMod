using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace Teuria.Profiles;

internal sealed class ArcherPortraitOptionsButton : MenuItem
{
    private ArcherData archerData;
    private Vector2 tweenFrom;
    private Vector2 tweenTo;
    private Action<string, ArcherData.ArcherTypes> action;

    public ArcherPortraitOptionsButton(Vector2 position, Vector2 tweenFrom, ArcherData archerData, Action<string, ArcherData.ArcherTypes> action) : base(position)
    {
        this.archerData = archerData;
        this.tweenFrom = tweenFrom;
        tweenTo = position;
        this.action = action;
    }

    public override void Render()
    {
        Color color = Selected ? OptionsButton.SelectedColor : OptionsButton.NotSelectedColor;
        Color portraitColor = Color.White;
        Subtexture currentPortrait = archerData.Portraits.Win;

        Draw.Texture(currentPortrait, Position, Color.White, 1f);
        Draw.Texture(currentPortrait, Position, portraitColor, 1f);
        Draw.HollowRect(
            new Rectangle((int)Position.X, (int)Position.Y,
            50, 50),
            color);
    }

    public override void TweenIn()
    {
        Position = tweenFrom;
        Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 20, true);
        tween.OnUpdate = t =>
        {
            Position = Vector2.Lerp(tweenFrom, tweenTo, t.Eased);
        };
        Add(tween);
    }

    public override void TweenOut()
    {
        Vector2 start = Position;
        Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeIn, 12, true);
        tween.OnUpdate = t =>
        {
            Position = Vector2.Lerp(start, tweenFrom, t.Eased);
        };
        Add(tween);
    }

    protected override void OnConfirm()
    {
        Sounds.ui_click.Play();
        var archerTypes = ArcherData.ArcherTypes.Normal;

        int idx = Array.IndexOf(ArcherData.Archers, archerData);
        if (idx == -1)
        {
            idx = Array.IndexOf(ArcherData.AltArchers, archerData);
            archerTypes = ArcherData.ArcherTypes.Alt;
            if (idx == -1)
            {
                idx = Array.IndexOf(ArcherData.SecretArchers, archerData);
                archerTypes = ArcherData.ArcherTypes.Secret;
            }
        }


        var entry = ProfilesModule.Instance.Context.Registry.Archers.RegisteredArchers
            .Select(x => x.Value)
            .FirstOrDefault(x => x.Index == idx);
        if (entry is null)
        {
            action(archerData.Name0 + archerData.Name1, archerTypes);
            return;
        }
        action(entry.Name, archerTypes);
    }

    protected override void OnDeselect()
    {
    }

    protected override void OnSelect()
    {
        if (MainMenu is not null)
        {
            MainMenu.TweenUICameraToY(Math.Max(0f, Y - 120f), 10);
        }
    }
}