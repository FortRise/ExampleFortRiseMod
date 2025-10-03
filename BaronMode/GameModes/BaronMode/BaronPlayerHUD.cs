using System;
using System.Collections.Generic;
using Teuria.BaronMode.Interop;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace Teuria.BaronMode.GameModes.UI;

public class BaronPlayerHUD : Entity
{
    public Facing Side;
    public int PlayerIndex;
    private ArcherData archerData;
    private List<Sprite<int>> gems;
    public bool HasGems => gems.Count > 0;

    public BaronPlayerHUD(BaronRoundLogic logic, Facing side, int playerIndex)
        : base(3)
    {
        Side = side;
        PlayerIndex = playerIndex;
        archerData = ArcherData.Get(TFGame.Characters[PlayerIndex], TFGame.AltSelect[PlayerIndex]);
        gems = new List<Sprite<int>>();

        if (playerIndex is 2 or 4 or 6) 
        {
            side = Facing.Left;
        }

        var uiOffsetX = EightPlayerUtils.GetUIOffset();

        for (int i = 0; i < logic.Lives[playerIndex]; i++)
        {
            Sprite<int> spriteInt = TFGame.SpriteData.GetSpriteInt(archerData.Gems.Gameplay);
            if (side == Facing.Left)
            {
                spriteInt.X = 8 + 10 * i - uiOffsetX;
            }
            else
            {
                spriteInt.X = 312 - (10 * i) + uiOffsetX;
            }
            spriteInt.Y = GetHeight(playerIndex);
            spriteInt.Play(0);
            gems.Add(spriteInt);
            Add(spriteInt);
        }
    }

    private float GetHeight(int index) 
    {
        if (index is 2 or 3) 
        {
            return 240 - 10;
        }
        if (index is 4 or 5) 
        {
            return 26 - 10;
        }
        if (index is 6 or 7) 
        {
            return 227 - 10;
        }
        return 13f;
    }

    public override void Render()
    {
        foreach (Sprite<int> gem in gems)
        {
            gem.DrawOutline();
        }
        base.Render();
    }

    public void GainLife(BaronRoundLogic logic) 
    {
        foreach (var gem in gems)
        {
            Remove(gem);
        }
        gems.Clear();

        var uiOffsetX = EightPlayerUtils.GetUIOffset();

        for (int i = 0; i < logic.Lives[PlayerIndex]; i++)
        {
            Sprite<int> spriteInt = TFGame.SpriteData.GetSpriteInt(archerData.Gems.Gameplay);
            if (Side == Facing.Left)
            {
                spriteInt.X = 8 + 10 * i - uiOffsetX;
            }
            else
            {
                spriteInt.X = 312 - (10 * i) + uiOffsetX;
            }
            spriteInt.Y = GetHeight(PlayerIndex);
            spriteInt.Play(0);
            gems.Add(spriteInt);
            Add(spriteInt);
        }
    }

    public void SpendLife(TeamReviver reviver)
    {
        Sounds.sfx_respawn1Gem.Play();
        Sprite<int> sprite = gems[^1];
        gems.RemoveAt(gems.Count - 1);

        var uiOffsetX = EightPlayerUtils.GetUIOffset();
        var reviverPos = new Vector2(reviver.Position.X - uiOffsetX, reviver.Position.Y);
        Vector2 start = sprite.Position;
        Vector2 end = reviverPos + Vector2.UnitY * -40f;

        end.Y = Math.Max(end.Y, 30f);
        Tween rotateTween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeInOut, 30, start: true);
        rotateTween.OnUpdate = t =>
        {
            sprite.Position = Vector2.Lerp(start, end, t.Eased);
            sprite.Scale = Vector2.One * MathHelper.Lerp(1f, 2f, t.Eased);
            sprite.Rotation = MathHelper.Pi * -2f * t.Eased;
        };
        rotateTween.OnComplete = t =>
        {
            Alarm.Set(this, 10, () =>
            {
                // duped, so it prevents caputuring
                var uiOffsetX = EightPlayerUtils.GetUIOffset();
                var reviverPos = new Vector2(reviver.Position.X - uiOffsetX, reviver.Position.Y);

                Vector2 final = reviverPos + Vector2.UnitY * -10f;
                Tween finalTween = Tween.Create(Tween.TweenMode.Oneshot, Ease.BackIn, 20, start: true);
                finalTween.OnUpdate = t =>
                {
                    sprite.Position = Vector2.Lerp(end, final, t.Percent);
                    sprite.Scale = Vector2.One * MathHelper.Lerp(2f, 0f, t.Eased);
                };
                finalTween.OnComplete = _ => Remove(sprite);
                
                Add(finalTween);
            });
        };
        Add(rotateTween);
    }
}
