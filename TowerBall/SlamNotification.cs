using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace TowerBall;

public class SlamNotification : Entity
{
	private float x;
	private float y;
	private Counter removeCounter;
	private Counter flashCounter;


	public SlamNotification(float x, float y)
		: base(3)
	{
		this.x = x;
		this.y = y;
		removeCounter = new Counter(120);
		flashCounter = new Counter(3);
	}

	public override void Update()
	{
		if (removeCounter)
		{
			removeCounter.Update();
		}
		else
		{
			RemoveSelf();
		}
		flashCounter.Update();
		if (!flashCounter)
		{
			Visible = !Visible;
			flashCounter.Set(3);
		}
	}

	public override void Render()
	{
		Draw.OutlineTextCentered(TFGame.Font, "DUNK!", new Vector2(x - TowerBallModModule.TestHalfWideUI, y), Color.Yellow, 2f);
	}
}


