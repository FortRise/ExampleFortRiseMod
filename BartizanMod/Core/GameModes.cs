using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FortRise;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using TowerFall;

namespace BartizanMod;

public class Respawn : IVersusGameMode
{
    public static IVersusGameModeEntry RespawnEntry { get; private set; } = null!; 
    public string Name => "Respawn";
    public Color NameColor => Color.Yellow;

    public Subtexture Icon => TFGame.MenuAtlas["Bartizan/gamemodes/respawn"];

    public bool IsTeamMode => false;

    public static void Register(IModRegistry registry)
    {
        RespawnEntry = registry.GameModes.RegisterVersusGameMode(new Respawn());
    }

    public RoundLogic OnCreateRoundLogic(Session session)
    {
        return new RespawnRoundLogic(session);
    }

    public Sprite<int> OverrideCoinSprite(Session session)
    {
        return DeathSkull.GetSprite();
    }

    public int OverrideCoinOffset(Session? session)
    {
        return 12;
    }

    public void OnStartGame(Session session)
    {
    }
}

public class Crawl : IVersusGameMode
{
    public static IVersusGameModeEntry CrawlEntry { get; private set; } = null!;

    public string Name => "Crawl";
    public Color NameColor => Color.Purple;

    public Subtexture Icon => TFGame.MenuAtlas["Bartizan/gamemodes/crawl"];

    public bool IsTeamMode => false;

    public static void Register(IModRegistry registry)
    {
        CrawlEntry = registry.GameModes.RegisterVersusGameMode(new Crawl());
    }

    public RoundLogic OnCreateRoundLogic(Session session)
    {
        return new MobRoundLogic(session);
    }

    public Sprite<int> OverrideCoinSprite(Session session)
    {
        return DeathSkull.GetSprite();
    }

    public int OverrideCoinOffset(Session? session)
    {
        return 12;
    }

    public void OnStartGame(Session session)
    {
    }
}


public class RespawnRoundLogic : RoundLogic
{
    private KillCountHUD[] killCountHUDs;
    private bool wasFinalKill;
    private Counter endDelay;
    private float[] autoReviveCounters;


    internal static void Load() 
    {
        On.TowerFall.RoundLogic.FFACheckForAllButOneDead += FFACheckForAllButOneDead_patch;
    }

    internal static void Unload() 
    {
        On.TowerFall.RoundLogic.FFACheckForAllButOneDead -= FFACheckForAllButOneDead_patch;
    }

    private static bool FFACheckForAllButOneDead_patch(On.TowerFall.RoundLogic.orig_FFACheckForAllButOneDead orig, RoundLogic self)
    {
        if (self is RespawnRoundLogic)
            return false;
        return orig(self);
    }


    public RespawnRoundLogic(Session session) :base(session, false)
    {
        var playerCount = EightPlayerUtils.GetPlayerCount();
        killCountHUDs = new KillCountHUD[playerCount];
        for (int i = 0; i < playerCount; i++) 
        {
            if (TFGame.Players[i]) 
            {
                killCountHUDs[i] = new KillCountHUD(i);
                this.Session.CurrentLevel.Add(killCountHUDs[i]);
            }
        }
        this.endDelay = new Counter();
        this.endDelay.Set(90);
        autoReviveCounters = new float[playerCount];
    }

    public override void OnLevelLoadFinish()
    {
        base.OnLevelLoadFinish();
        Session.CurrentLevel.Add<VersusStart>(new VersusStart(base.Session));
        Players = SpawnPlayersFFA();
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
        if (BartizanModModule.Instance.Settings.RespawnMode == BartizanModSettings.Delayed && !Session.CurrentLevel.Ending) 
        {
            for (int i = 0; i < autoReviveCounters.Length; i++) 
            {
                if (this.autoReviveCounters[i] > 0f)
                {
                    this.autoReviveCounters[i] -= Engine.TimeMult;
                    if (this.autoReviveCounters[i] <= 0f)
                    {
                        this.DoAutoRevive(i);
                    }
                }
            }
        }
    }

    private void DoAutoRevive(int playerIndex)
    {
        TeamReviver? selectedTeamReviver = null;
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
            selectedTeamReviver.AutoRevive = true;
        }
    }

    protected Player RespawnPlayer(int playerIndex)
    {
        List<Vector2> spawnPositions = this.Session.CurrentLevel.GetXMLPositions("PlayerSpawn");

        var player = new Player(playerIndex, new Random().Choose(spawnPositions), Allegiance.Neutral, Allegiance.Neutral,
                        this.Session.GetPlayerInventory(playerIndex), this.Session.GetSpawnHatState(playerIndex),
                        frozen: false, flash: false, indicator: true);
        this.Session.CurrentLevel.Add(player);
        player.Flash(120, null);
        Alarm.Set(player, 60, player.RemoveIndicator, Alarm.AlarmMode.Oneshot);
        return player;
    }

    protected virtual void AfterOnPlayerDeath(Player player, PlayerCorpse corpse)
    {
        if (BartizanModModule.Instance.Settings.RespawnMode == BartizanModSettings.Instant)
            this.RespawnPlayer(player.PlayerIndex);
        else
            Session.CurrentLevel.Add(new TeamReviver(corpse, TeamReviver.Modes.Quest));
    }

    public override void OnPlayerDeath(Player player, PlayerCorpse corpse, int playerIndex, DeathCause cause, Vector2 position, int killerIndex)
    {
        base.OnPlayerDeath(player, corpse, playerIndex, cause, position, killerIndex);

        if (killerIndex == playerIndex || killerIndex == -1) {
            killCountHUDs[playerIndex].Decrease();
            base.AddScore(playerIndex, -1);
        } else if (killerIndex != -1) {
            killCountHUDs[killerIndex].Increase();
            base.AddScore(killerIndex, 1);
        }

        int winner = base.Session.GetWinner();
        if (this.wasFinalKill && winner == -1) {
            this.wasFinalKill = false;
            base.Session.CurrentLevel.Ending = false;
            base.CancelFinalKill();
            this.endDelay.Set(90);
        }
        if (!this.wasFinalKill && winner != -1) {
            base.Session.CurrentLevel.Ending = true;
            this.wasFinalKill = true;
            base.FinalKill(corpse, winner);
        }

        autoReviveCounters[playerIndex] = 60f;

        this.AfterOnPlayerDeath(player, corpse);
    }
}


public class MobRoundLogic : RespawnRoundLogic
{
    private PlayerGhost[] activeGhosts;

    public MobRoundLogic(Session session)
        : base(session)
    {
        var playerCount = EightPlayerUtils.GetPlayerCount();
        activeGhosts = new PlayerGhost[playerCount];
    }

    protected override void AfterOnPlayerDeath(Player player, PlayerCorpse corpse)
    {
    }

    private void RemoveGhostAndRespawn(int playerIndex, Vector2 position = default)
    {
        var ghost = activeGhosts[playerIndex];
        if (ghost != null && ghost.Scene != null) 
        {
            var player = this.RespawnPlayer(playerIndex);
            if (player == null)
                return;

            // if we've been given a position, make sure the ghost spawns at that position and
            // retains its speed pre-spawn.
            if (position != default) 
            {
                player.Position.X = position.X;
                player.Position.Y = position.Y;

                player.Speed.X = ghost.Speed.X;
                player.Speed.Y = ghost.Speed.Y;
            }
            ghost.RemoveSelf();
            ghost = null;
        }
    }

    private IEnumerator GhostSpawnSequence(PlayerCorpse corpse, int playerIndex, Vector2 position, int killerIndex)
    {
        while (corpse.Squished != Vector2.Zero || corpse.Revived)
        {
            yield return null;
        }
        Session.CurrentLevel.Add(activeGhosts[playerIndex] = new PlayerGhost(corpse));

        if (killerIndex == playerIndex || killerIndex == -1) 
        {
            if (this.Session.CurrentLevel.LivingPlayers == 0) 
            {
                var otherPlayers = TFGame.Players.Select((playing, idx) => playing && idx != playerIndex ? (int?)idx : null).Where(idx => idx != null).ToList();
                var randomPlayer = new Random().Choose(otherPlayers)!.Value;
                RemoveGhostAndRespawn(randomPlayer);
            }
        } 
        else 
        {
            RemoveGhostAndRespawn(killerIndex, position);
        }
    }

    public override void OnPlayerDeath(Player player, PlayerCorpse corpse, int playerIndex, DeathCause cause, Vector2 position, int killerIndex)
    {
        base.OnPlayerDeath(player, corpse, playerIndex, cause, position, killerIndex);
        
        if (DynamicData.For(corpse).TryGet<Coroutine>("ghostMobCoroutine", out var val)) 
        {
            val.Replace(GhostSpawnSequence(corpse, playerIndex, position, killerIndex));
            val.Active = true;
        }
        // this.Session.CurrentLevel.Add(activeGhosts[playerIndex] = new PlayerGhost(corpse));
    }
}

public class KillCountHUD : Entity
{
    private int playerIndex;
    private List<Sprite<int>> skullIcons = new();

    public int Count { get { return this.skullIcons.Count; } }

    public KillCountHUD(int playerIndex)
        : base(3)
    {
        this.playerIndex = playerIndex;
    }

    public void Increase()
    {
        Sprite<int> sprite = DeathSkull.GetSprite();
        sprite.Color = ArcherData.GetColorA(playerIndex);

        var width = EightPlayerUtils.GetScreenWidth();

        if (this.playerIndex % 2 == 0) 
            sprite.X = 8 + 10 * skullIcons.Count;
        else 
            sprite.X = width - 8 - 10 * skullIcons.Count;
        float offset = 0;
        if (playerIndex > 4)
            offset = 20;

        sprite.Y = this.playerIndex / 2 is 0 or 2 or 4 ? 20 + offset : (240 - 20) - offset;
        sprite.Stop();
        this.skullIcons.Add(sprite);
        base.Add(sprite);
    }

    public void Decrease()
    {
        if (this.skullIcons.Any()) 
        {
            base.Remove(this.skullIcons.Last());
            this.skullIcons.Remove(this.skullIcons.Last());
        }
    }

    public override void Render()
    {
        foreach (Sprite<int> sprite in this.skullIcons) 
        {
            sprite.DrawOutline(1);
        }
        base.Render();
    }
}

public static class MyPlayerGhost 
{
    internal static void Load() 
    {
        On.TowerFall.PlayerGhost.ctor += ctor_patch;
        On.TowerFall.PlayerGhost.Die += Die_patch;
    }

    internal static void Unload() 
    {
        On.TowerFall.PlayerGhost.ctor -= ctor_patch;
        On.TowerFall.PlayerGhost.Die -= Die_patch;
    }

    private static void ctor_patch(On.TowerFall.PlayerGhost.orig_ctor orig, TowerFall.PlayerGhost self, PlayerCorpse corpse)
    {
        DynamicData.For(self).Set("corpse", corpse);
        orig(self, corpse);
    }

    private static void Die_patch(On.TowerFall.PlayerGhost.orig_Die orig, TowerFall.PlayerGhost self, int killerIndex, Arrow arrow, Explosion explosion, ShockCircle shock)
    {
        orig(self, killerIndex, arrow, explosion, shock);
        var mobLogic = self.Level.Session.RoundLogic as MobRoundLogic;
        if (mobLogic != null && DynamicData.For(self).TryGet<PlayerCorpse>("corpse", out var corpse)) 
        {
            mobLogic.OnPlayerDeath(null!, corpse, self.PlayerIndex, DeathCause.Arrow, self.Position, killerIndex);
        }
    }
}

public static class MyPlayerCorpse 
{
    public static void Load() 
    {
        On.TowerFall.PlayerCorpse.ctor_string_Allegiance_Vector2_Facing_int_int += ctor_patch;
        On.TowerFall.PlayerCorpse.Update += Update_patch;
    }

    public static void Unload() 
    {
        On.TowerFall.PlayerCorpse.ctor_string_Allegiance_Vector2_Facing_int_int -= ctor_patch;
        On.TowerFall.PlayerCorpse.Update -= Update_patch;
    }

    private static void ctor_patch(On.TowerFall.PlayerCorpse.orig_ctor_string_Allegiance_Vector2_Facing_int_int orig, PlayerCorpse self, string corpseSpriteID, Allegiance teamColor, Vector2 position, Facing facing, int playerIndex, int killerIndex)
    {
        orig(self, corpseSpriteID, teamColor, position, facing, playerIndex, killerIndex);
        var level = (Engine.Instance.Scene as Level)!;
        if (level.Session.MatchSettings.CustomVersusModeName == Crawl.CrawlEntry.Name) 
        {
            var dynSelf = DynamicData.For(self);
            var coroutine = new Coroutine();
            dynSelf.Set("ghostMobCoroutine", coroutine);
        }
    }

    private static void Update_patch(On.TowerFall.PlayerCorpse.orig_Update orig, PlayerCorpse self)
    {
        orig(self);
        if (DynamicData.For(self).TryGet<Coroutine>("ghostMobCoroutine", out var val) && val.Active) 
        {
            val.Update();
        }
    }
}