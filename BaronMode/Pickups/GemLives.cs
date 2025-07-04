using Teuria.BaronMode.GameModes;
using FortRise;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace Teuria.BaronMode.Pickups;

public class GemLives : Pickup, IRegisterable
{
    public static ISubtextureEntry GemPickupTexture { get; private set; } = null!;
    public static IPickupEntry GemLivesMeta = null!;
    public static void Register(IModContent content, IModRegistry registry)
    {
        GemPickupTexture = registry.Subtextures.RegisterTexture(
            content.Root.GetRelativePath("Content/pickups/gemCoin.png")
        );
        GemLivesMeta = registry.Pickups.RegisterPickups("GemLives", new() 
        {
            Name = "Gem Lives",
            PickupType = typeof(GemLives)
        });
    }

    private Sprite<int> sprite;
    private float time;
    private int count;

    public GemLives(Vector2 position, Vector2 targetPosition) : base(position, targetPosition)
    {
        Collider = new Hitbox(16f, 16f, -8f, -8f);
        Tag(GameTags.PlayerCollectible);
        sprite = new Sprite<int>(GemPickupTexture.Subtexture, 12, 10);
        sprite.Add(0, 0.1f, [0, 0, 0, 1, 2, 3, 4, 5, 6, 7]);
    }

    public override void Added()
    {
        base.Added();
        Player? firstPlayerAlive = null;
        foreach (Player player in Level.Players)
        {
            if (player.Dead)
            {
                continue;
            }
            firstPlayerAlive = player;
            break;
        }
        if (firstPlayerAlive != null)
        {
            SetGemSprite(firstPlayerAlive.PlayerIndex);
        }
        else 
        {
            SetGemSprite(-1);
        }
    }

    public void SetGemSprite(int playerIndex) 
    {
        if (playerIndex == -1)
        {
            sprite = new Sprite<int>(GemPickupTexture.Subtexture, 12, 10);
            sprite.Add(0, 0.1f, [0, 0, 0, 1, 2, 3, 4, 5, 6, 7]);
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
            var player = Level.GetPlayer(count);
            if (player != null)
            {
                SetGemSprite(player.PlayerIndex);
            }
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
                Level.Add(new FloatText(
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
                Level.Add(new FloatText(
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
            Level.Add(new FloatText(
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

        Level.Add(Cache.Create<LightFade>().Init(this));
        RemoveSelf();
    }

    private static int GetPlayerRangeCount() 
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