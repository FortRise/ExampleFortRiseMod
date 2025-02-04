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
            if (logic.Overtime)
            {
                Sounds.pu_darkOrbCollect.Play(X);
                Level.Add<FloatText>(new FloatText(
                    Position, "IT BROKE!", 
                    player.ArcherData.ColorA,
                    player.ArcherData.ColorB,
                    1f, 0.5f, false));
                Level.Particles.Emit(new ParticleType
                {
                    Color = player.ArcherData.ColorA,
                    Color2 = player.ArcherData.ColorB,
                    ColorSwitch = 4,
                    Size = 2f,
                    SizeRange = 3f,
                    Speed = 1f,
                    SpeedRange = 0.7f,
                    SpeedMultiplier = 0.91f,
                    DirectionRange = 6.2831855f,
                    Life = 26,
                    LifeRange = 12
                }, 12, Position, Vector2.One * 4f);
                
            }
            else 
            {
                logic.AddLife(player.PlayerIndex);
                Level.Add<FloatText>(new FloatText(
                    Position, "+1 LIFE", 
                    player.ArcherData.ColorA,
                    player.ArcherData.ColorB,
                    1f, 0.5f, false));
                Sounds.sfx_gemCollect.Play(X);
            }
        }
        else 
        {
            Sounds.pu_darkOrbCollect.Play(X);
            Level.Add<FloatText>(new FloatText(
                Position, "IT BROKE!", 
                player.ArcherData.ColorA,
                player.ArcherData.ColorB,
                1f, 0.5f, false));
            Level.Particles.Emit(new ParticleType
            {
                Color = player.ArcherData.ColorA,
                Color2 = player.ArcherData.ColorB,
                ColorSwitch = 4,
                Size = 2f,
                SizeRange = 3f,
                Speed = 1f,
                SpeedRange = 0.7f,
                SpeedMultiplier = 0.91f,
                DirectionRange = 6.2831855f,
                Life = 26,
                LifeRange = 12
            }, 12, Position, Vector2.One * 4f);
        }

        Level.Add<LightFade>(Cache.Create<LightFade>().Init(this));
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