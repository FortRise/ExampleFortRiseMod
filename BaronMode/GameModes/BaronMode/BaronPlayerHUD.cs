using System;
using System.Collections.Generic;
using BaronMode.Interop;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace BaronMode.GameModes.UI;

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

        if (playerIndex is 2 or 4 or 6) {
            side = Facing.Left;
        }

        var width = EightPlayerUtils.GetScreenWidth() - 8;
        for (int i = 0; i < logic.Lives[playerIndex]; i++)
        {
            Sprite<int> spriteInt = TFGame.SpriteData.GetSpriteInt(archerData.Gems.Gameplay);
            if (side == Facing.Left)
            {
                spriteInt.X = 8 + 10 * i;
            }
            else
            {
                spriteInt.X = width - 10 * i;
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

    public void GainLives(BaronRoundLogic logic) 
    {
        foreach (var gem in gems)
        {
            Remove(gem);
        }
        gems.Clear();

        var width = EightPlayerUtils.GetScreenWidth() - 8;

        for (int i = 0; i < logic.Lives[PlayerIndex]; i++)
        {
            Sprite<int> spriteInt = TFGame.SpriteData.GetSpriteInt(archerData.Gems.Gameplay);
            if (Side == Facing.Left)
            {
                spriteInt.X = 8 + 10 * i;
            }
            else
            {
                spriteInt.X = width - 10 * i;
            }
            spriteInt.Y = GetHeight(PlayerIndex);
            spriteInt.Play(0);
            gems.Add(spriteInt);
            Add(spriteInt);
        }
    }

    public void LoseLives()
    {
        for (int i = 0; i < gems.Count; i++)
        {
            Sprite<int> sprite = gems[gems.Count - 1 - i];
            Vector2 end;
            Vector2 control;
            if (Side == Facing.Left)
            {
                end = sprite.Position + new Vector2(-60f, 40f);
                control = sprite.Position + new Vector2(40f, 10f);
            }
            else
            {
                end = sprite.Position + new Vector2(60f, 40f);
                control = sprite.Position + new Vector2(-40f, 10f);
            }
            Bezier bezier = new Bezier(sprite.Position, end, control);
            bezier.Begin = sprite.Position;

            var tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineIn, 30 + i * 4, start: true);
            tween.OnUpdate = (t) =>
            {
                sprite.Position = bezier.GetPoint(t.Eased);
                sprite.Rotation = MathHelper.Pi * 4f * (float)Side * Ease.SineOut(t.Percent);
            };
            Add(tween);
        }
        gems.Clear();
    }

    public void SpendLife(TeamReviver reviver)
    {
        Sounds.sfx_respawn1Gem.Play();
        Sprite<int> sprite = gems[gems.Count - 1];
        gems.RemoveAt(gems.Count - 1);
        Vector2 start = sprite.Position;
        Vector2 end = reviver.Position + Vector2.UnitY * -40f;
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
                Vector2 final = reviver.Position + Vector2.UnitY * -10f;
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