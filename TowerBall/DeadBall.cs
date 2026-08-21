using System;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace TowerBall;

public class DeadBall : Entity
{
	private Vector2 Speed;
	private OutlineImage ballImage;
	private Counter killCounter;
	private Counter flashCounter;
	private WrapHitbox hitbox;
    

	public DeadBall(Vector2 pos, Vector2 s)
		: base(pos)
	{
		Position = pos;
		Speed = s;
		killCounter = new Counter(60);
		flashCounter = new Counter(3);
		ballImage = new OutlineImage(TFGame.Atlas["Suyooo.TowerBall/towerball/ball"])
		{
			Origin = new Vector2(5f, 5f)
		};
		Add(ballImage);
		hitbox = new WrapHitbox(6f, 6f, -3f, -3f);
	}

	public bool DoesCollide()
	{
		var list = Scene[GameTags.Solid];
		hitbox.Position = Position;

		for (int i = 0; i < list.Count; i++)
		{
			if (hitbox.Collide(list[i].Collider) && list[i] is Platform && !(list[i] is BasketBallBasket))
			{
				return true;
			}
		}

		return false;
	}

	public override void Update()
	{
		Speed.X = Calc.Approach(Speed.X, 0f, 0.025f * Engine.TimeMult);
		Speed.Y = Math.Min(Speed.Y + 0.2f * Engine.TimeMult, 5.5f);
		Position.X += Speed.X * Engine.TimeMult;

		if (DoesCollide())
		{
			while (DoesCollide())
			{
				Position.X -= Math.Sign(Speed.X);
				if (Speed.X == 0f)
				{
					Position.X = Calc.Approach(Position.X, 160f, 1f);
				}
			}
			Speed.X *= -0.8f;
		}

		Position.Y += Speed.Y * Engine.TimeMult;
		if (DoesCollide())
		{
			while (DoesCollide())
			{
				Position.Y -= Math.Sign(Speed.Y);
				if (Speed.Y == 0f)
				{
					Position.Y = Calc.Approach(Position.Y, 0f, 1f);
				}
			}
			Speed.Y *= -0.5f;
		}

		killCounter.Update();
		flashCounter.Update();
		if (!flashCounter)
		{
			Visible = !Visible;
			flashCounter.Set(3);
		}

		if (!killCounter)
		{
			RemoveSelf();
		}
	}
}

