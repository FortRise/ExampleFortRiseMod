using BaronMode.GameModes;
using FortRise;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace BaronMode.Pickups;

[CustomPickup("BaronMode/Lives", Chance = 0.6f)]
public class GemLives : Pickup
{
    private Sprite<int> sprite;
    private float time;
    private int count;

    public GemLives(Vector2 position, Vector2 targetPosition) : base(position, targetPosition)
    {
        base.Collider = new Hitbox(16f, 16f, -8f, -8f);
        base.Tag(GameTags.PlayerCollectible);
    }

    public override void Added()
    {
        base.Added();
        Player firstPlayerAlive = null;
        foreach (Player player in Level.Players)
        {
            if (player.Dead)
            {
                continue;
            }
            firstPlayerAlive = player;
            break;
        }
        SetGemSprite(firstPlayerAlive.PlayerIndex);

    }

    public void SetGemSprite(int playerIndex) 
    {
        if (playerIndex == -1)
        {
            sprite = new Sprite<int>(TFGame.Atlas["BaronMode/pickups/gemCoin"], 12, 10);
        }
        else 
        {
            sprite = TFGame.SpriteData.GetSpriteInt(Level.GetPlayer(playerIndex).ArcherData.Gems.Gameplay);
        }

        sprite.Play(0, false);
        Add(sprite);
    }

    public override void Update()
    {
        base.Update();
        sprite.Position = DrawOffset;

        if (time >= 1) 
        {
            var rangeCount = GetPlayerRangeCount();
            time = 0;
            while (!Level.IsPlayerAlive(count) && Level.LivingPlayers != 0)
            {
                count += 1;
                if (count > rangeCount)
                {
                    count = 0;
                }
            }

            
            Remove(sprite);
            SetGemSprite(Level.GetPlayer(count).PlayerIndex);
            count += 1;

            if (count > rangeCount)
            {
                count = 0;
            }
            return;
        }
        time += Engine.TimeMult * 0.05f;
    }

    public override void Render()
    {
        DrawGlow();
        sprite.DrawOutline();
        base.Render();
    }

    public override void OnPlayerCollide(Player player)
    {
        DoCollectStats(player.PlayerIndex);
        if (Level.Session.RoundLogic is BaronRoundLogic logic)
        {
            logic.AddLife(player.PlayerIndex);
            Level.Add<FloatText>(new FloatText(
                Position, "+1 LIFE", 
                player.ArcherData.ColorA,
                player.ArcherData.ColorB,
                1f, 0.5f, false));
        }
        else 
        {
            player.HasShield = true;
            player.HasWings = true;
            ShockCircle shockCircle = Cache.Create<ShockCircle>();
            shockCircle.Init(player.Position, player.PlayerIndex, player, ShockCircle.ShockTypes.BoltCatch);
            Level.Add<ShockCircle>(shockCircle);

			Sounds.sfx_reviveRedteamFinish.Play(player.Position.X);
            Level.Add<FloatText>(new FloatText(
                Position, "ULTIMATE POWERUP", 
                player.ArcherData.ColorA,
                player.ArcherData.ColorB,
                1f, 0.5f, false));
        }

        Level.Add<LightFade>(Cache.Create<LightFade>().Init(this));
        Sounds.sfx_gemCollect.Play(X);
        RemoveSelf();
    }

    private int GetPlayerRangeCount() 
    {
        int range = 0;
        for (int i = 0; i < TFGame.Players.Length; i++)
        {
            if (TFGame.Players[i]) 
            {
                range = i;
            }
        }

        return range;
    }
}