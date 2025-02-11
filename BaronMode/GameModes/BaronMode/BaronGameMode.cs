using System;
using System.Collections;
using System.Collections.Generic;
using BaronMode.GameModes.UI;
using BaronMode.Interop;
using FortRise;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using TowerFall;

namespace BaronMode.GameModes;

public class Baron : CustomGameMode
{
    private int[] totalLives;

    public override void StartGame(Session session) 
    {
        var playerCount = EightPlayerUtils.GetMenuPlayerCount();
        totalLives = new int[playerCount];
        var goal = session.MatchSettings.GoalScore;

        for (int i = 0; i < playerCount; i++) 
        {
            if (TFGame.Players[i]) 
            {
                session.Scores[i] = goal;
                session.OldScores[i] = goal;
                totalLives[i] = goal;
            }
            else    
            {
                totalLives[i] = -1;    
            }
        }
    }

    public override RoundLogic CreateRoundLogic(Session session)
    {
        return new BaronRoundLogic(session, totalLives);
    }

    public override void Initialize()
    {
        Icon = TFGame.Atlas["BaronMode/gameModes/baron"];
        NameColor = Color.LightPink;
        CoinOffset = 12;
    }

    public override void InitializeSounds() {}

    public override Sprite<int> CoinSprite()
    {
        var sprite = new Sprite<int>(TFGame.Atlas["BaronMode/pickups/gemCoin"], 12, 10);

        sprite.Add(0, 0.1f, new int[] { 0, 0, 0, 1, 2, 3, 4, 5, 6, 7});
        sprite.Play(0);
        sprite.CenterOrigin();
        return sprite;
    }
}

public class BaronRoundLogic : RoundLogic
{
    private Counter endDelay;
    private Entity coroutineHolder;
    private float anotherTreasureSpawn;
    private float[] autoReviveCounters;
    private BaronPlayerHUD[] PlayerHUDs;
    private TeamReviver[] teamRevivers;
    private bool wasFinalKill;
    private int[] totalLives;
    public int[] Lives;
    public bool Overtime;

    public BaronRoundLogic(Session session, int[] totalLives) : base(session, false) 
    {
        coroutineHolder = new Entity();
        var playerCount = EightPlayerUtils.GetPlayerCount();
        endDelay = new Counter();
        endDelay.Set(90);
        autoReviveCounters = new float[playerCount];
        PlayerHUDs = new BaronPlayerHUD[playerCount];
        teamRevivers = new TeamReviver[playerCount];
        Lives = new int[playerCount];
        anotherTreasureSpawn = 1200;
        this.totalLives = totalLives;
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
                if (totalLives[k] > BaronModeModule.Instance.Settings.BaronLivesCount)
                    Lives[k] = BaronModeModule.Instance.Settings.BaronLivesCount;
                else
                    Lives[k] = totalLives[k];
                Session.CurrentLevel.Add(this.PlayerHUDs[k] = new BaronPlayerHUD(this, (k == 0) ? Facing.Left : Facing.Right, k));
            }
            else 
            {
                Lives[k] = -1;
            }
        }
    }

    internal static void Load() 
    {
        On.TowerFall.RoundLogic.FFACheckForAllButOneDead += FFACheckForAllButOneDead_patch;
        On.TowerFall.Session.GetWinner += GetWinner_patch;
        On.TowerFall.Session.ShouldSpawn += ShouldSpawn_patch;
        On.TowerFall.PlayerCorpse.CanDoPrismHit += CanDoPrismHit_patch;
    }

    internal static void Unload() 
    {
        On.TowerFall.RoundLogic.FFACheckForAllButOneDead -= FFACheckForAllButOneDead_patch;
        On.TowerFall.Session.GetWinner -= GetWinner_patch;
        On.TowerFall.Session.ShouldSpawn -= ShouldSpawn_patch;
        On.TowerFall.PlayerCorpse.CanDoPrismHit -= CanDoPrismHit_patch;
    }

    private static bool CanDoPrismHit_patch(On.TowerFall.PlayerCorpse.orig_CanDoPrismHit orig, PlayerCorpse self, Arrow arrow)
    {
        // Do not remove the corpse as it prevents it from respawning
        if (arrow.Level.Session.RoundLogic is BaronRoundLogic) 
        {
            return false;
        }
        return orig(self, arrow);
    }

    private static bool ShouldSpawn_patch(On.TowerFall.Session.orig_ShouldSpawn orig, Session self, int playerIndex)
    {
        if (self.RoundLogic is BaronRoundLogic logic) 
        {
            if (logic.totalLives[playerIndex] > -1) 
            {
                return true;
            }
            return false;
        }
        return orig(self, playerIndex);
    }

    private static int GetWinner_patch(On.TowerFall.Session.orig_GetWinner orig, Session self)
    {
        if (self.RoundLogic is BaronRoundLogic logic) 
        {
            int alive = 0;
            int lastAlive = 0;
            for (int i = 0; i < logic.totalLives.Length; i++) 
            {
                if (logic.totalLives[i] <= -1) 
                    continue;
                alive++;
                lastAlive = i;
            }
            if (alive == 1) 
            {
                return lastAlive;
            }
            if (alive == 0) 
            {
                for (int i = 0; i < logic.totalLives.Length; i++) 
                {
                    foreach (Monocle.Entity entity in self.CurrentLevel[Monocle.GameTags.Player])
                    {
                        Player player2 = (Player)entity;
                        if (!player2.Dead)
                        {
                            return player2.PlayerIndex;
                        }
                    }
                }
            }
            return -1;
        }
        return orig(self);
    }

    private static bool FFACheckForAllButOneDead_patch(On.TowerFall.RoundLogic.orig_FFACheckForAllButOneDead orig, RoundLogic self)
    {
        if (self is BaronRoundLogic)
            return false;
        return orig(self);
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        if (base.RoundStarted && base.Session.CurrentLevel.Ending && base.Session.CurrentLevel.CanEnd) 
        {
            if (this.endDelay) 
            {
                this.endDelay.Update();
                return;
            }
            base.Session.EndRound();
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
        totalLives[playerIndex] += 1;

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
            treasure.Untag(new Monocle.GameTags[]
			{
				Monocle.GameTags.PlayerCollider,
				Monocle.GameTags.PlayerGhostCollider
			});
            listToRemove.Add(treasure);
            yield return 40;
        }
        Session.CurrentLevel.Remove(listToRemove);
        yield return 120;
        base.SpawnTreasureChestsVersus();
        yield break;
    }

    private void DoAutoRevive(int playerIndex)
    {
        TeamReviver selectedTeamReviver = null;
        foreach (TeamReviver teamReviver in base.Session.CurrentLevel[GameTags.TeamReviver])
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
                    PlayerHUDs[playerIndex].SpendLife(selectedTeamReviver);
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
            if (totalLives[playerIndex] > 0) 
            {
                totalLives[playerIndex]--;
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
            if (Session.MatchSettings.LevelSystem.Theme.World == TowerTheme.Worlds.Dark)
            {
                Music.Play("DarkBoss");
            }
            else
            {
                Music.Play("Boss");
            }
            Session.CurrentLevel.Add<FloatText>(
                new FloatText(corpse.Position + new Vector2(0f, -8f), "OVERTIME", 
                Color.Red, Color.White, 1f, 1f, false));
            Overtime = true;
            return;
        }


        if (Lives[playerIndex] > 0) 
        {
            totalLives[playerIndex]--;
            Lives[playerIndex]--;
            autoReviveCounters[playerIndex] = 60f;
            AddScore(playerIndex, -1);
            Session.CurrentLevel.Add<FloatText>(
                new FloatText(corpse.Position + new Vector2(0f, -8f), "-1 LIFE", 
                ArcherData.GetColorA(playerIndex), Color.Red, 1f, 1f, false));
            return;
        }
        if (totalLives[playerIndex] == 0)
        {
            totalLives[playerIndex]--;
        }
        Lives[playerIndex] = -1;
        Session.CurrentLevel.Add<FloatText>(
            new FloatText(corpse.Position + new Vector2(0f, -3f), "DEAD", 
            ArcherData.GetColorA(playerIndex), Color.DarkRed, 1f, 1f, false));

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
                FinalKill(corpse, winnerIndex);
                return;
            }

            FinalKillNoSpotlightOrMusicStop();
        }
    }

    public override void OnRoundStart()
    {
        base.OnRoundStart();
        base.SpawnTreasureChestsVersus();
    }
}