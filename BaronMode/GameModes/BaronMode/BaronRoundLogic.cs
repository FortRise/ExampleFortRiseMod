using System;
using System.Collections;
using System.Collections.Generic;
using Teuria.BaronMode.GameModes.UI;
using Teuria.BaronMode.Interop;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using TowerFall;
using System.Linq;

namespace Teuria.BaronMode.GameModes;

public class BaronRoundLogic : RoundLogic
{
    private readonly Counter endDelay;
    private readonly Entity coroutineHolder;
    private readonly float[] autoReviveCounters;
    private readonly float[] reviverTimeoutCounters;
    private readonly BaronPlayerHUD[] PlayerHUDs;
    private readonly TeamReviver[] teamRevivers;

    private bool wasFinalKill;
    public int[] TotalLives;
    public int[] Lives;
    public bool Overtime;
    private float anotherTreasureSpawn;

    public BaronRoundLogic(Session session, int[] totalLives) : base(session, false) 
    {
        coroutineHolder = new Entity();
        var playerCount = EightPlayerUtils.GetPlayerCount();
        endDelay = new Counter();
        endDelay.Set(90);
        autoReviveCounters = new float[playerCount];
        reviverTimeoutCounters = new float[playerCount];
        PlayerHUDs = new BaronPlayerHUD[playerCount];
        teamRevivers = new TeamReviver[playerCount];
        Lives = new int[playerCount];
        anotherTreasureSpawn = 1200;
        TotalLives = totalLives;
    }

    public override void OnLevelLoadFinish()
    {
        base.OnLevelLoadFinish();
        Session.CurrentLevel.Add(coroutineHolder);

        Session.CurrentLevel.Add(new VersusStart(Session));
        Players = SpawnPlayersFFA();

        var playerCount = EightPlayerUtils.GetPlayerCount();

        for (int k = 0; k < playerCount; k++)
        {
            if (TFGame.Players[k])
            {
                if (TotalLives[k] > BaronModeModule.Instance.Settings.BaronLivesCount)
                {
                    Lives[k] = BaronModeModule.Instance.Settings.BaronLivesCount;
                }
                else
                {
                    Lives[k] = TotalLives[k];
                }

                Session.CurrentLevel.Add(PlayerHUDs[k] = new BaronPlayerHUD(this, (k == 0) ? Facing.Left : Facing.Right, k));
            }
            else 
            {
                Lives[k] = -1;
            }
        }
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        if (RoundStarted && Session.CurrentLevel.Ending && Session.CurrentLevel.CanEnd) 
        {
            if (endDelay) 
            {
                endDelay.Update();
                return;
            }
            Session.EndRound();
        }
        if (!Session.CurrentLevel.Ending) 
        {
            if (!Overtime)
            {
                if (anotherTreasureSpawn > 0f) 
                {
                    anotherTreasureSpawn -= Engine.TimeMult;
                }
                else 
                {
                    anotherTreasureSpawn = 1200;
                    coroutineHolder.Add(new Coroutine(SpawnTreasure()));
                }
            }

            for (int i = 0; i < autoReviveCounters.Length; i++) 
            {
                if (autoReviveCounters[i] > 0f)
                {
                    autoReviveCounters[i] -= Engine.TimeMult;
                    if (autoReviveCounters[i] <= 0f)
                    {
                        DoAutoRevive(i);
                    }
                }

                if (reviverTimeoutCounters[i] > 0 && teamRevivers[i] != null && !teamRevivers[i].Finished)
                {
                    reviverTimeoutCounters[i] -= Engine.TimeMult;
                    if (reviverTimeoutCounters[i] <= 0f)
                    {
                        teamRevivers[i].AutoRevive = false;
                        Lives[i] = -1;
                        CheckWin(null);
                    }
                }
            }
        }
    }

    public void AddLife(int playerIndex)
    {
        if (Overtime)
        {
            return;
        }
        AddScore(playerIndex, 1);
        Lives[playerIndex] = Math.Min(BaronModeModule.Instance.Settings.BaronLivesCount, Lives[playerIndex] + 1);
        TotalLives[playerIndex] += 1;

        PlayerHUDs[playerIndex].GainLife(this);
    }

    private IEnumerator SpawnTreasure() 
    {
        var listToRemove = new List<Entity>();
        foreach (var pickup in Session.CurrentLevel[GameTags.LightSource]) 
        {
            if (pickup is Pickup) 
            {
                Session.CurrentLevel.Particles.Emit(Particles.PlayerDust[5], 12, pickup.Position, new Vector2(5f, 8f));
                listToRemove.Add(pickup);
            }
        }
        Session.CurrentLevel.Remove(listToRemove);
        yield return 60;
        listToRemove.Clear();
        for (int i = 0; i < Session.CurrentLevel[GameTags.TreasureChest].Count; i++) 
        {
            var treasure = Session.CurrentLevel[GameTags.TreasureChest][i];
            Sounds.sfx_cyanWarp.Play(treasure.X, 1f);
            Session.CurrentLevel.Particles.Emit(Particles.PlayerDust[5], 12, treasure.Position, new Vector2(5f, 8f));
            treasure.Active = false;
            treasure.Visible = false;
            DynamicData.For(treasure).Set("State", TreasureChest.States.Opened);
            treasure.Untag(
            [
                GameTags.PlayerCollider,
				GameTags.PlayerGhostCollider
			]);
            listToRemove.Add(treasure);
            yield return 40;
        }
        Session.CurrentLevel.Remove(listToRemove);
        yield return 120;
        SpawnTreasureChestsVersus();
        yield break;
    }

    private void DoAutoRevive(int playerIndex)
    {
        TeamReviver? selectedTeamReviver = null;
        foreach (TeamReviver teamReviver in Session.CurrentLevel[GameTags.TeamReviver].Cast<TeamReviver>())
        {
            if (teamReviver.Corpse.PlayerIndex == playerIndex)
            {
                selectedTeamReviver = teamReviver;
                break;
            }
        }
        if (selectedTeamReviver != null && !selectedTeamReviver.AutoRevive)
        {
            teamRevivers[playerIndex] = selectedTeamReviver;
            try 
            {
                if (Lives[playerIndex] != -1)
                {
                    PlayerHUDs[playerIndex].SpendLife(selectedTeamReviver);
                    reviverTimeoutCounters[playerIndex] = 360;
                }
            }
            catch 
            {
                // Very unnecessary anyway
            }

            selectedTeamReviver.AutoRevive = true;
        }
    }

    public override void OnPlayerDeath(Player player, PlayerCorpse corpse, int playerIndex, DeathCause cause, Vector2 position, int killerIndex)
    {
        base.OnPlayerDeath(player, corpse, playerIndex, cause, position, killerIndex);

        Session.CurrentLevel.Add(new TeamReviver(corpse, TeamReviver.Modes.Quest));

        if (wasFinalKill && Session.CurrentLevel.LivingPlayers == 0) 
        {
            if (TotalLives[playerIndex] > 0) 
            {
                TotalLives[playerIndex]--;
                Lives[playerIndex]--;
                AddScore(playerIndex, -1);
                return;
            }
            endDelay.Set(90);
            Session.CurrentLevel.Ending = false;
            var playerCount = EightPlayerUtils.GetPlayerCount();
            CancelFinalKill();

            for (int i = 0; i < playerCount; i++) 
            {
                if (TFGame.Players[i]) 
                {
                    autoReviveCounters[i] = 60f;
                }
            }

            if (Session.MatchSettings.LevelSystem!.Theme.World == TowerTheme.Worlds.Dark)
            {
                Music.Play("DarkBoss");
            }
            else
            {
                Music.Play("Boss");
            }
            Session.CurrentLevel.Add(
                new FloatText(corpse.Position + new Vector2(0f, -8f), "OVERTIME", 
                Color.Red, Color.White, 1f, 1f, false));
            Overtime = true;
            for (int i = 0; i < Lives.Length; i++) 
            {
                Lives[i] = 0;
            }
            return;
        }


        if (Lives[playerIndex] > 0) 
        {
            TotalLives[playerIndex]--;
            Lives[playerIndex]--;
            autoReviveCounters[playerIndex] = 60f;
            AddScore(playerIndex, -1);
            Session.CurrentLevel.Add(
                new FloatText(corpse.Position + new Vector2(0f, -8f), "-1 LIFE", 
                ArcherData.GetColorA(playerIndex), Color.Red, 1f, 1f, false));
            return;
        }
        if (TotalLives[playerIndex] == 0)
        {
            TotalLives[playerIndex]--;
        }
        Lives[playerIndex] = -1;
        Session.CurrentLevel.Add(
            new FloatText(corpse.Position + new Vector2(0f, -3f), "DEAD", 
            ArcherData.GetColorA(playerIndex), Color.DarkRed, 1f, 1f, false));

        CheckWin(corpse);
    }

    private void CheckWin(PlayerCorpse? corpse)
    {
        int alive = 0;

        for (int i = 0; i < Lives.Length; i++) 
        {
            var life = Lives[i];
            if (life != -1) 
            {
                alive++;
                continue;
            }
        }
        if (alive <= 1) 
        {
            Session.CurrentLevel.Ending = true;
            var winnerIndex = Session.GetWinner();
            if (winnerIndex != -1) 
            {
                wasFinalKill = true;
                if (corpse != null)
                {
                    FinalKill(corpse, winnerIndex);
                }
                return;
            }

            FinalKillNoSpotlightOrMusicStop();
        }
    }

    public override void OnRoundStart()
    {
        base.OnRoundStart();
        SpawnTreasureChestsVersus();
    }
}
