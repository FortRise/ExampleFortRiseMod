using System;
using System.Collections.Generic;
using System.Timers;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace TowerBall;

public class TowerBallRoundLogic : RoundLogic
{
	public Vector2 ballPos;
	public Alarm spawnAlarm = null!;
	public PlayerRespawner[] respawns;
	private Counter endDelay;
	public TowerBallHUD hud = null!;
	public List<Arrow> arrowQueue;
	public int LastThrower;
	public Timer roundTimer = null!;
	public bool overtime = false;
	public bool timed = false;
	public int secondsleft;

	public TowerBallRoundLogic(Session session)
		: base(session, false)
	{
		CanMiasma = false;
		respawns = new PlayerRespawner[4];
		arrowQueue = new List<Arrow>();

		endDelay = new Counter();
		endDelay.Set(90);
	}

    public override void OnRoundStart()
    {
        base.OnRoundStart();
        SpawnTreasureChestsVersus();

        if (timed)
		{
            roundTimer.Enabled = true;
        }
    }

    public override void OnLevelLoadFinish()
	{
		base.OnLevelLoadFinish();
		Session.CurrentLevel.Add(new VersusStart(Session));
		SpawnPlayersTeams();

		Players = TFGame.PlayerAmount;
		var xMLPositions = Session.CurrentLevel.GetXMLPositions("BigTreasureChest");
		ballPos = xMLPositions[0];
		SpawnBallChest();

		var xMLPositions2 = Session.CurrentLevel.GetXMLPositions("Basket");
		Session.CurrentLevel.Add(new BasketBallBasket(null!, xMLPositions2[0]));
		Session.CurrentLevel.Add(new BasketBallBasket(null!, xMLPositions2[1]));
		hud = Session.CurrentLevel.Add(new TowerBallHUD(this));

		Session.MatchSettings.Variants!.ReturnAsGhosts.Value = false;
		Session.MatchSettings.Variants.TriggerCorpses.Value = false;
		overtime = false;
		Session.MatchSettings.Variants.TeamRevive.Value = false;
		//if (Session.MatchSettings.Variants.GetCustomVariant("TowerBall/TimedRounds"))
		//{
  //          roundTimer = new Timer(1000)
  //          {
  //              AutoReset = true
  //          };
  //          secondsleft = 30 * Session.MatchSettings.GoalScore;
		//	roundTimer.Elapsed += TimeDecrement;
		//	
		//
  //          timed = true;
		//}
		//else
		//{
		//	timed = false;
		//}
	}

	public override void OnUpdate()
	{
		for (int i = 0; i < arrowQueue.Count; i++)
		{
			Session.CurrentLevel.Add(arrowQueue[i]);
		}

		arrowQueue.Clear();
		SessionStats.TimePlayed += Engine.DeltaTicks;
		base.OnUpdate();
	
        if (secondsleft <= 0 && timed && !Session.CurrentLevel.Ending)
        {
            RoundEndTimed();
        } 
        
		if (RoundStarted && Session.CurrentLevel.Ending && Session.CurrentLevel.CanEnd)
		{
			if (endDelay)
			{
				endDelay.Update();
				return;
			}
			
			Session.EndRound();
		}
		else
		{
			InsertCrownEvent();
		}
	}


	private void TimeDecrement(object source, ElapsedEventArgs e)
	{
		secondsleft--;
	}

	public Player RespawnPlayer(int playerIndex, Allegiance team)
	{
		List<Vector2> list = (team != 0) ? Session.CurrentLevel.GetXMLPositions("TeamSpawnB") : Session.CurrentLevel.GetXMLPositions("TeamSpawnA");
		Player player = new Player(playerIndex, list[new Random().Next(list.Count)], team, team, Session.GetPlayerInventory(playerIndex), Session.GetSpawnHatState(playerIndex), frozen: false, flash: false, indicator: true);
		Session.CurrentLevel.Add(player);
		player.Flash(120);
		Alarm.Set(player, 60, player.RemoveIndicator);
		return player;
	}
	public void RoundEndTimed()
	{
		if (Session.Scores[1] == Session.Scores[0])
		{
			overtime = true;
			return;
		}

		Allegiance allegiance = Allegiance.Blue;
		if (Session.Scores[1] == Session.GetHighestScore())
		{
			allegiance = Allegiance.Red;
		}

		List<LevelEntity> list = [];

		for (int i = 0; i < 4; i++)
		{
			if (TFGame.Players[i] && Session.MatchSettings.Teams![i] == allegiance)
			{
				Session.MatchStats[i].GotWin = true;
				LevelEntity playerOrCorpse = Session.CurrentLevel.GetPlayerOrCorpse(i);
				if (playerOrCorpse != null)
				{
					list.Add(playerOrCorpse);
				}
			}
		}
		if (Session.Scores[(int)allegiance] < Session.MatchSettings.GoalScore)
		{
			Session.Scores[(int)allegiance] = Session.MatchSettings.GoalScore;
		}

        Session.CurrentLevel.LightingLayer.SetSpotlight([.. list]);
		Session.CurrentLevel.OrbLogic.DoSlowMoKill();
		Session.MatchSettings.LevelSystem!.StopVersusMusic();

		hud.RemoveSelf();
		Session.CurrentLevel.CanEnd = true;
		Session.CurrentLevel.Ending = true;
	}
    public override void OnPlayerDeath(Player player, PlayerCorpse corpse, int playerIndex, DeathCause cause, Vector2 position, int killerIndex)
	{
		Entity entity = new Entity();
		Alarm.Set(entity, 180, () => {
			RespawnPlayer(playerIndex, player.Allegiance);
			entity.RemoveSelf();
		});
		Session.CurrentLevel.Add(entity);
		if ((bool)Session.CurrentLevel.KingIntro)
		{
			Session.CurrentLevel.KingIntro.Laugh();
		}
		if ((bool)Session.MatchSettings.Variants.GunnStyle)
		{
			Session.CurrentLevel.Add(new GunnStyle(corpse));
		}
		if (killerIndex != -1 && killerIndex != playerIndex)
		{
			Kills[killerIndex]++;
		}
		if (killerIndex != -1 && !Session.CurrentLevel.IsPlayerAlive(killerIndex))
		{
			Session.MatchStats[killerIndex].KillsWhileDead = Session.MatchStats[killerIndex].KillsWhileDead + 1;
		}
		if (killerIndex != -1 && killerIndex != playerIndex)
		{
			Session.MatchStats[killerIndex].RegisterFastestKill(Time);
		}
		DeathType deathType = DeathType.Normal;
		if (killerIndex == playerIndex)
		{
			deathType = DeathType.Self;
		}
		else if (killerIndex != -1 && Session.MatchSettings.TeamMode && Session.MatchSettings.GetPlayerAllegiance(playerIndex) == Session.MatchSettings.GetPlayerAllegiance(killerIndex))
		{
			deathType = DeathType.Team;
		}
		if (killerIndex != -1 && deathType == DeathType.Normal && Session.WasWinningAtStartOfRound(playerIndex))
		{
			Session.MatchStats[killerIndex].WinnerKills = Session.MatchStats[killerIndex].WinnerKills + 1;
		}
		DeathType type = deathType;
		int characterIndex = ((killerIndex == -1) ? (-1) : TFGame.Characters[killerIndex]);
		Session.MatchStats[playerIndex].Deaths.Add(type, cause, characterIndex);
		SaveData.Instance.Stats.Deaths.Add(deathType, cause, TFGame.Characters[playerIndex]);
		if (!Session.MatchSettings.SoloMode)
		{
			SessionStats.RegisterVersusKill(killerIndex, playerIndex, deathType == DeathType.Team);
		}
	}
	
	public void IncreaseScore(int playerIndex, int assistPlayerIndex, bool dunk, bool allyoop, bool clean, int teamIndex)
	{
		if (endDelay.Value != 90)
		{
			return;
		}
		AddScore(teamIndex, 1);
		TFGame.PlayerInputs[playerIndex].Rumble(1f, 10);
		hud.FlashAlpha = 1f;
		if (!timed)
		{
			if (Session.GetHighestScore() < Session.MatchSettings.GoalScore)
			{
                //if (base.Session.MatchSettings.Variants.GetCustomVariant("TowerBall/HoopTreasure"))
                //{
                //    SpawnTreasureChestsVersus();
                //}
                return;
			}
		}
		else
		{
			if (!overtime)
			{
                //if (base.Session.MatchSettings.Variants.GetCustomVariant("TowerBall/HoopTreasure"))
                //{
                //    SpawnTreasureChestsVersus();
                //}
                return;
			}
        }
		Allegiance allegiance = Allegiance.Blue;
		if (Session.Scores[1] == Session.GetHighestScore())
		{
			allegiance = Allegiance.Red;
		}
		
		List<LevelEntity> list = new List<LevelEntity>();
		for (int i = 0; i < 4; i++)
		{
			if (TFGame.Players[i] && Session.MatchSettings.Teams[i] == allegiance)
			{
				Session.MatchStats[i].GotWin = true;
				LevelEntity playerOrCorpse = Session.CurrentLevel.GetPlayerOrCorpse(i);
				if (playerOrCorpse != null)
				{
					list.Add(playerOrCorpse);
				}
			}
		}
		
		if (overtime)
		{
            if (Session.Scores[(int)allegiance] < Session.MatchSettings.GoalScore)
            {
                Session.Scores[(int)allegiance] = Session.MatchSettings.GoalScore;
            }
        }
        Session.CurrentLevel.LightingLayer.SetSpotlight([.. list]);
		Session.CurrentLevel.OrbLogic.DoSlowMoKill();
		Session.MatchSettings.LevelSystem!.StopVersusMusic();

		Sounds.sfx_finalKill.Play();

        hud.RemoveSelf();
        Session.CurrentLevel.CanEnd = true;
        Session.CurrentLevel.Ending = true;
    }

	public void AddDeadBall(Vector2 pos, Vector2 s)
	{
		Session.CurrentLevel.Add(new DeadBall(pos, s));
	}

	public void AddSlamNotification(Vector2 pos)
	{
		Session.CurrentLevel.Add(new SlamNotification(pos.X + 7f, pos.Y));
	}

	public void SpawnBallChest()
	{
		Session.CurrentLevel.Add(new BasketBallTreasureChest(null!, ballPos));
		LastThrower = -1;
	}

	public void SpawnBallChest(int delay)
	{
        var entity = new Entity();
        var alarm = Alarm.Set(entity, delay, SpawnBallChest);
        alarm.Start();
        Session.CurrentLevel.Add(entity);
	}

	public void DropBall(Player p, Vector2 pos, Facing face)
	{
		Arrow ball = Arrow.Create(BasketBall.BasketBallEntry.ArrowTypes, p, pos, (face == Facing.Right) ? 0f : ((float)Math.PI));
		ball.Drop((int)face);
		ball.Speed.Y *= 0.5f;
		arrowQueue.Add(ball);
	}

	public void DropBall(Player p, Vector2 pos)
	{
		Arrow ball = Arrow.Create(BasketBall.BasketBallEntry.ArrowTypes, p, pos, 4.712389f);
		ball.Drop(0);
		ball.Speed.X = ball.Speed.Y = 0f;
		arrowQueue.Add(ball);
	}
}
