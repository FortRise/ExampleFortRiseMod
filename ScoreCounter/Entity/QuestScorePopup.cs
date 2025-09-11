using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace Teuria.ScoreCounter;

internal sealed class QuestScorePopup : Entity
{
    private Color color;
    private readonly Sprite<int> sprite;
    private readonly Tween appearTween;
    private readonly Tween disappearTween;

    private float startY;

    public QuestScorePopup() : base(3) 
    {
        sprite = TFGame.SpriteData.GetSpriteInt("QuestPoints");
        Add(sprite);

        disappearTween = Tween.Create(Tween.TweenMode.Persist, Ease.SineIn, 20, true);
        disappearTween.OnUpdate = t => 
        { 
            Y = MathHelper.Lerp(startY, startY - 20, t.Eased);
            sprite.Color = color * (1f - t.Eased);
        };

        Add(disappearTween);

        appearTween = Tween.Create(Tween.TweenMode.Persist, Ease.BackOut, 20, true);
        appearTween.OnUpdate = t => sprite.Scale = Vector2.One * t.Eased;
        appearTween.OnComplete = t =>
        {
            startY = Y;
            disappearTween.Start();
        };

        Add(appearTween);
    }


    public void Initialize(Vector2 position, int amount, int playerIndex)
    {
        Depth = -(amount / 100);
        Position = position;
        color = ArcherData.GetColorB(playerIndex);

        sprite.Play(amount / 100 - 1, true);
        sprite.Color = color;
        sprite.Scale = Vector2.Zero;

        Alarm.Set(this, 10, appearTween.Start);

        appearTween.Stop();
        disappearTween.Stop();
    }


    public override void Removed()
    {
        base.Removed();
        Cache.Store(this);
    }
}
