using System;
using FortRise;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace TowerBall;

public class BasketBall : ToyArrow
{
    private float prevXSpeed;
	private Image ballImage = null!;


	public Allegiance ThrownTeam;
	public Wiggler scaleWiggler;
	public Counter cantCollectCounter;
	public Counter slamming = null!;
	public Hitbox ballColliderNormal;
	public Hitbox ballColliderOtherArrow;

    public int AssistIndex;

	public bool allyOop;

	private bool fallModeEntered;
    private bool cleanShot;

    public override bool IsCollectible
    {
        get 
        {
            if (cantCollectCounter)
            {
                return false;
            }

            return Speed.Length() < 2f;
        }
    }

    public BasketBall()
    {
        LightRadius = 80f;
        cleanShot = true;
        ThrownTeam = Allegiance.Neutral;
        cantCollectCounter = new Counter(8);

        ballColliderNormal = new Hitbox(8f, 8f, -4f, -4f);
        ballColliderOtherArrow = new Hitbox(12f, 12f, -6f, -6f);

        Collider = ballColliderNormal;
        TargetCollider = ballColliderNormal;

        scaleWiggler = Wiggler.Create(20, 4f, null, (v) 
            => ballImage!.Scale = Vector2.One * (1f + 0.2f * v));
        Add(scaleWiggler);
    }

    public void JustSpawned()
    {
        cantCollectCounter.Set(10);
    }

    public static IArrowEntry BasketBallEntry { get; private set; } = null!;

    public static void Register(IModuleContext context)
    {
        var ball = context.Registry.Subtextures.GetTexture("Suyooo.TowerBall/towerball/ball", SubtextureAtlasDestination.Atlas);
        BasketBallEntry = context.Registry.Arrows.RegisterArrows("BasketBall", new() 
        {
            ArrowPickupName = "BASKETBALL",
            CreateArrow = () => new BasketBall(),
            HUD = ball
        });
    }

    protected override void CreateGraphics()
    {
        ballImage = new Image(TFGame.Atlas["Suyooo.TowerBall/towerball/ball"])
        {
            Origin = Vector2.One * 5f
        };
        Graphics = [ballImage];
        Add(Graphics);
    }

    protected override void InitGraphics()
    {
        ballImage.Visible = true;
    }

    protected override bool CheckForTargetCollisions()
    {
        if (cantCollectCounter)
        {
            return false;
        }

        for (int i = 0; i < 4; i += 1)
        {
            var player = Level.GetPlayer(i);

            if (player is not null && player.ArrowCheck(this) && player != CannotHit)
            {
                if (cantCollectCounter <= 0 && (player.Allegiance == ThrownTeam ||
                        ThrownTeam == Allegiance.Neutral))
                {
                    player.CollectArrows(BasketBallEntry.ArrowTypes);
                    player.ArcherData.SFX.ArrowRecover.Play(player.X);
                    RemoveSelf();
                    return false;
                }
            }
        }

        foreach (Entity entity in Level[GameTags.Target])
        {
            var cannotHitEnemiesCounter = Private.Field<Arrow, Counter>("cannotHitEnemiesCounter", this).Read();
            if (entity is LevelEntity levelEntity && !cannotHitEnemiesCounter && entity is not TreasureChest)
            {
                if (levelEntity.ArrowCheck(this) && levelEntity != CannotHit && levelEntity.OnArrowHit(this))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void ForceArrowState()
    {
        State = ArrowStates.Shooting;
        var noSeekingArrowsActive = false;

        if (PlayerIndex >= 0)
        {
            noSeekingArrowsActive = 
                Level.Session.MatchSettings.Variants!.NoSeekingArrows[PlayerIndex];
            Level.Session.MatchSettings.Variants.NoSeekingArrows[PlayerIndex] = true;
        }

        ShootUpdate();
        if (PlayerIndex >= 0)
        {
            Level.Session.MatchSettings.Variants!.NoSeekingArrows[PlayerIndex] 
                = noSeekingArrowsActive;
        }
    }

    protected override void OnCollideH(Platform platform)
    {
        if (platform.Collidable)
        {
            if (Math.Abs(Speed.X) > 1.5f)
            {
                TowerBallSFX.BallSFX.SFX!.Play(X, Math.Min(Math.Abs(Speed.Y / 5f), 1f));
            }

            prevXSpeed *= -1f;
            int bounce = (int)Math.Round(Math.Abs(Speed.X)) / 2;
            int xAxis = -Math.Sign(Speed.X);

            if (bounce > 0)
            {
                Level.Particles.Emit(
                    Particles.PlayerDust[3], 
                    bounce, 
                    Position + new Vector2(4 * -xAxis, 0f),
                    new Vector2(1f, 3f),
                    new Vector2(xAxis, -0.5f).Angle()
                );
            }

            ForceArrowState();
            cleanShot = true;
        }
    }

    protected override void OnCollideV(Platform platform)
    {
        if (platform is BasketBallBasket basket)
        {
            var isRight = Position.X > 160f;
            if ((isRight && Position.X <= platform.Position.X) || 
                (!isRight && Position.X >= platform.Position.X + 15f))
            {
				if (Math.Abs(Speed.Y) > 1.5f)
				{
                    TowerBallSFX.BallSFX.SFX!.Play(X, Math.Min(Math.Abs(Speed.Y / 5f), 1f));
				}
				float length = Speed.Length();
				float angleRadians = Calc.Angle(platform.Position + new Vector2((!isRight) ? 15 : 0, 0f), Position);

				Speed = Calc.AngleToVector(angleRadians, length);
				prevXSpeed = Speed.X + (isRight ? (-0.1f) : 0.1f);
				Speed.Y *= -0.9f;
				cleanShot = false;
            }
            else if (Speed.Y > 0f)
            {
                Sounds.sfx_devTimeFinalDummy.Play(X);

                var roundLogic = (TowerBallRoundLogic)Level.Session.RoundLogic;
                roundLogic.IncreaseScore(PlayerIndex, AssistIndex, slamming, allyOop, cleanShot, (!isRight) ? 1 : 0);
                roundLogic.SpawnBallChest(300);

                if ((isRight && Position.X - 5f < platform.Position.X) || (!isRight && Position.X + 5f >= platform.Position.X + 15f))
                {
                    Speed.X *= -1f;
                }
                else
                {
                    Speed.X = MathHelper.Lerp(0f, prevXSpeed, Math.Max(Speed.Y / 2f, 1f));
                }

                roundLogic.AddDeadBall(Position, Speed);

                if (slamming)
                {
                    Explosion.Spawn(
                        Level,
                        platform.Position + new Vector2(7.5f, 0f),
                        PlayerIndex, plusOneKill: false, triggerBomb: false, bombTrap: false);

                    roundLogic.AddSlamNotification(
                        platform.Position + (isRight ?
                            new Vector2(-10, 0f) : new Vector2(10f, 0f)));

                    if (Level.KingIntro)
                    {
                        Level.KingIntro.Laugh();
                    }
                }

                basket.DoNetJump();
                RemoveSelf();
            }
        }
        else if (platform.Collidable)
        {
            if (Math.Abs(Speed.Y) > 1.5f)
            {
                TowerBallSFX.BallSFX.SFX!.Play(X, Math.Min(Math.Abs(Speed.Y / 5f), 1f));
            }

            Speed.Y *= -0.9f;

            if (Math.Abs(Speed.Y) < 1f)
            {
                Speed.Y = 0f;
            }
            else if (Math.Abs(Speed.Y) > 1.75f)
            {
                int bounce = (int)Math.Round(Math.Abs(Speed.Y)) / 2;
                if (bounce > 0)
                {
                    Level.Particles.Emit(
                        Particles.PlayerDust[3],
                        bounce,
                        Position + new Vector2(0f, 4f),
                        Vector2.One * 3f
                    );
                }
            }

            if (Math.Abs(Speed.Y) > 0.5f)
            {
                HitWall(platform);
            }

            ForceArrowState();
            cleanShot = false;
        }
    }

    public override void EnterFallMode(bool bounce = true, bool zeroX = false, bool sound = true)
    {
        fallModeEntered = true;
        base.EnterFallMode(bounce, zeroX, sound);
        Speed.X *= 2f;

        if (Speed.X != 0f)
        {
            return;
        }

        if (X < 160f)
        {
            Speed.X = 1f;
        }
        else
        {
            Speed.X = -1f;
        }
    }

	public override void Update()
	{
		prevXSpeed = Speed.X;
		float rotation = ballImage.Rotation;
		if (slamming)
		{
			slamming.Update();
			if (Speed.Y <= 0.1f)
			{
				slamming.Set(0);
			}
		}

		if (cleanShot && Math.Abs(Speed.X) == 0f)
		{
			cleanShot = false;
		}

		if (cantCollectCounter)
		{
			cantCollectCounter.Update();
		}

		bool noArrowsActive = false;
		if (PlayerIndex >= 0) 
		{
			noArrowsActive = Level.Session.MatchSettings.Variants!.NoSeekingArrows[PlayerIndex];
			Level.Session.MatchSettings.Variants.NoSeekingArrows[PlayerIndex] = true;
		}

		fallModeEntered = false;
		base.Update();

		if (PlayerIndex >= 0)
		{
			Level.Session.MatchSettings.Variants!.NoSeekingArrows[PlayerIndex] = noArrowsActive;
		}
		if (fallModeEntered)
		{
			prevXSpeed = Speed.X;
		}

        ForceArrowState();

		if (State == ArrowStates.Gravity)
		{
			Speed.X = Calc.Approach(prevXSpeed, 0f, 0.025f * Engine.TimeMult);
		}
		else
		{
			Speed.X = prevXSpeed;
		}
		ballImage.Rotation = rotation + Speed.X * 0.002f;
	}

    public override void HitLava() {}

    protected override void HitWall(Platform platform)
    {
        if (CollideFirst(GameTags.Mud) is Mud mud)
        {
            Speed.X = Calc.Approach(prevXSpeed, 0f, 25f * Engine.TimeMult);
            prevXSpeed = Speed.X;
            Speed.Y += 0.5f;
            mud.SplashDown(X);
            Sounds.env_mudArrowLand.Play(X, 1f);
        }
        else if (CollideCheck(GameTags.HotCoals, Position + Vector2.UnitY))
        {
            Sounds.sfx_coalBurn.Play(X, 1f);
            Speed.Y -= 1f;
            Fire.Start();
        }
    }
}

