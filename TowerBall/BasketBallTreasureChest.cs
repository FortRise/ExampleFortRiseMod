using System;
using System.Xml;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace TowerBall;

public class BasketBallTreasureChest : TreasureChest
{
	private Counter openCounter;
	private Counter spawnCounter;

    private bool hasSpawned;
    private bool shouldOpened;

	public BasketBallTreasureChest(XmlElement xml, Vector2 position)
		: base(position, Types.Special, AppearModes.Normal, [])
	{
		openCounter = new Counter();
		openCounter.Set(60);
		spawnCounter = new Counter();
		Flash(15);
        
    }

	public bool ReadyToAppear()
	{
		return true;
	}

	public override void Update()
	{
		base.Update();
		if (spawnCounter && !hasSpawned)
		{
			spawnCounter.Update();
			if (spawnCounter)
			{
				return;
			}
			Player? player = null;
			for (int i = 0; i < 4; i++)
			{
				player = Level.GetPlayer(i);
				if (player != null)
				{
					break;
				}
			}

			if (player != null)
			{
				Allegiance allegiance = player.Allegiance;
				player.Allegiance = Allegiance.Neutral;
				Arrow entity = Arrow.Create(
                    BasketBall.BasketBallEntry.ArrowTypes, player, Position, (float)Math.PI / 2f);
				player.Allegiance = allegiance;

				Level.Add(entity);
                hasSpawned = true;

                Flash(30, RemoveSelf);
			}
            else 
            {
                spawnCounter.Set(1);
            }
		}
		else if (openCounter && Level.Session.RoundLogic.RoundStarted)
		{
			openCounter.Update();
			if (!openCounter)
			{
                spawnCounter.Set(1);
			}
		}
	}

	public override bool OnArrowHit(Arrow arrow)
	{
		return false;
	}

	public override void OnPlayerCollide(Player player)
	{
	}

	public override void OnPlayerGhostCollide(PlayerGhost player)
	{
	}
}

