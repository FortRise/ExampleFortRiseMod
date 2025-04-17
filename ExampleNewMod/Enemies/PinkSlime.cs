using System;
using FortRise;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using TowerFall;

namespace ExampleNewMod;

public class PinkSlime : Slime
{
    private PlayerShield? shield;
    private DynData<Slime> slimeData;
    private float jumpAround;
    public PinkSlime(Vector2 position, Facing facing, bool hasShield) : base(position, facing, SlimeColors.Red)
    {
        slimeData = new DynData<Slime>(this);
        if (hasShield) 
        {
            shield = new PlayerShield(this);
            Add(shield);
        }


        slimeData.Set("walkSpeed", 0.8f);
        var sprite = slimeData.Get<Sprite<string>>("sprite");
        Remove(sprite);

        var pinkSprite = TFGame.SpriteData.GetSpriteString("ExampleNewMod/PinkSlime");
        pinkSprite.OnAnimationComplete = (Sprite<string> s) =>
        {
            if (State == 3)
            {
                State = 0;
            }
        };
        slimeData.Set("sprite", pinkSprite);
        pinkSprite.Play("idle");
        Add(pinkSprite);
    }

    public static void Register(IModRegistry registry)
    {
        registry.Enemies.RegisterEnemy("SuperSlime", new() 
        {
            Name = "Super Slime",
            Loader = (pos, facing, _) => new PinkSlime(pos, facing, false)
        });

        registry.Enemies.RegisterEnemy("SuperSlimeS", new() 
        {
            Name = "Super Slime Shield",
            Loader = (pos, facing, _) => new PinkSlime(pos, facing, true)
        });
    }

    public override void Hurt(Vector2 force, int damage, int killerIndex, Arrow? arrow = null, Explosion? explosion = null, ShockCircle? shock = null)
    {
        if (shield != null) 
        {
            Speed = force;
            Flash(30, null);
            shield.Lose();
            Remove(shield);
            shield = null;
            if (arrow != null) 
            {
                arrow.EnterFallMode(true, false, true);
                return;
            }
            return;
        }
        base.Hurt(force, damage, killerIndex, arrow, explosion, shock);
    }

    internal static void Load() 
    {
        On.TowerFall.Slime.WalkUpdate += Slime_WalkUpdate_patch;
    }

    internal static void Unload() 
    {
        On.TowerFall.Slime.WalkUpdate -= Slime_WalkUpdate_patch;
    }

    private static int Slime_WalkUpdate_patch(On.TowerFall.Slime.orig_WalkUpdate orig, Slime self)
    {
        if (self is PinkSlime pink)
        {
            if (pink.ShouldTurnAround())
            {
                pink.jumpAround = 0.2f * (int)pink.Facing;
                return 2;
            }
        }
        return orig(self);
    }

    public override void Added()
    {
        if (shield != null && shield)
        {
            shield.Gain();
        }
        base.Added();
    }

    private bool ShouldTurnAround()
    {
        return base.CollideCheck(GameTags.Solid, this.Position + Vector2.UnitX * (float)this.Facing * 2f);
    }

    private int AirUpdate()
    {
        if (base.CheckBelow() && this.Speed.Y >= 0f)
        {
            return 0;
        }
        float target = 0f;
        Player closestPlayerWithSight = base.GetClosestPlayerWithSight(6400f);
        if (closestPlayerWithSight)
        {
            float value = WrapMath.DiffX(base.X, closestPlayerWithSight.X);
            if (Math.Abs(value) <= 60f)
            {
                target = (float)Math.Sign(value) * 0.6f;
            }
        }
        Speed.X = Calc.Approach(this.Speed.X, target + jumpAround, 0.04f * Engine.TimeMult);
        Speed.Y = Calc.Approach(this.Speed.Y, 1.8f, 0.15f * Engine.TimeMult);
        return 1;
    }
}

