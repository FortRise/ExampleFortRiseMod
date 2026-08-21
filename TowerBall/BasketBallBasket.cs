using System.Xml;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace TowerBall;

public class BasketBallBasket : JumpThru
{
    public OutlineImage Rim;
    public OutlineImage Net;
    public Tween? NetJump;

    public BasketBallBasket(XmlElement xml, Vector2 position) 
        : base(position, 15)
    {
        var isRight = Position.X > 160f + TowerBallModModule.TestWideUI;

        Position.X -= 10f;
        Position.Y += 10f;

        if (isRight)
        {
            Position.X += 5f;
        }

        Depth += 1;

        Rim = new OutlineImage(TFGame.Atlas["Suyooo.TowerBall/towerball/rim"]) 
        {
            Origin = Vector2.One,
            FlipX = isRight
        };

        Net = new OutlineImage(TFGame.Atlas["Suyooo.TowerBall/towerball/net"]) 
        {
            Origin = new Vector2(-3f, -2f),
            FlipX = isRight
        };

        if (isRight)
        {
            Rim.Position.X = 1f;
            Rim.Position.Y = -2f;
            Rim.OutlineColor = new Color(248, 120, 88);
        }
        else 
        {
            Rim.OutlineColor = new Color(164, 228, 252);
        }

        Add(Net);
        Add(Rim);
    }

    public override void Update()
    {
        base.Update();
        if (NetJump is not null)
        {
            NetJump.Update();
            Net.Scale.Y = NetJump.Eased * 0.5f + 0.5f;
        }
        else 
        {
            Net.Scale.Y = 1f;
        }
    }

    public void DoNetJump()
    {
        NetJump = Tween.Create(Tween.TweenMode.Oneshot, Ease.BounceOut, 40, true);
    }
}

