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
    private ProfileArcher profileArcher;
    private PlayerProfile profile;

    public bool HasArcher => profile.SelectedArchers.Contains(profileArcher);
    public int ArcherIndex => profile.SelectedArchers.IndexOf(profileArcher);
    

    public ArcherPortraitOptionsButton(
        Vector2 position, 
        Vector2 tweenFrom, 
        ArcherData archerData, 
        PlayerProfile profile
    ) : base(position)
    {
        this.archerData = archerData;
        this.tweenFrom = tweenFrom;
        tweenTo = position;
        this.profile = profile;

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
            profileArcher = new ProfileArcher(archerData.Name0 + archerData.Name1, archerTypes);
            return;
        }

        profileArcher = new ProfileArcher(entry.Name, archerTypes);
    }

    public override void Render()
    {
        Color color = Selected ? OptionsButton.SelectedColor : OptionsButton.NotSelectedColor;
        Color portraitColor = HasArcher ? Color.White : Color.Black * 0.5f;

        Subtexture currentPortrait = HasArcher ? archerData.Portraits.Win : archerData.Portraits.Lose;


        if (HasArcher)
        {
            Draw.Rect(
                new Rectangle((int)Position.X - 2, (int)Position.Y - 2,
                54, 54),
                OptionsButton.SelectedColor);
        }

        Draw.HollowRect(
            new Rectangle((int)Position.X - 1, (int)Position.Y - 1,
            52, 52),
            color);
        Draw.Texture(currentPortrait, Position + new Vector2(25, 25), Color.White, new Vector2(currentPortrait.Width / 2, currentPortrait.Height / 2), 1f, 0f);
        Draw.Texture(currentPortrait, Position + new Vector2(25, 25), portraitColor, new Vector2(currentPortrait.Width / 2, currentPortrait.Height / 2), 1f, 0f);

        var archerIndex = ArcherIndex;

        if (archerIndex != -1)
        {
            Draw.OutlineTextCentered(TFGame.Font, (archerIndex + 1).ToString(), new Vector2(Position.X + 45, Position.Y + 45), Color.White, Color.Black);
        }
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

        if (!profile.SelectedArchers.Remove(profileArcher))
        {
            profile.SelectedArchers.Add(profileArcher);
        }
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