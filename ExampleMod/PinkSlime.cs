using System.Reflection;
using FortRise;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using TowerFall;

namespace ExampleMod;

[CustomEnemyAttribute(
    "ExampleMod/SuperSlime = NoShield", 
    "ExampleMod/SuperSlimeS = ShieldUp"
)]
public class PinkSlime : Slime
{
    public static Slime NoShield(Vector2 position, Facing facing) 
        => new PinkSlime(position, facing, false);

    public static Slime ShieldUp(Vector2 position, Facing facing) 
        => new PinkSlime(position, facing, true);

    private PlayerShield shield;
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
    }


    public override void Hurt(Vector2 force, int damage, int killerIndex, Arrow arrow = null, Explosion explosion = null, ShockCircle shock = null)
    {
        if (this.shield != null) 
        {
            Speed = force;
            base.Flash(30, null);
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

    public override void Load()
    {
        slimeData.Set<float>("walkSpeed", 0.8f);
        var sprite = slimeData.Get<Sprite<string>>("sprite");
        Remove(sprite);

        var pinkSprite = ExampleModModule.Data.GetSpriteString("PinkSlime");
        pinkSprite.OnAnimationComplete = (Sprite<string> s) =>
        {
            if (base.State == 3)
            {
                base.State = 0;
            }
        };
        slimeData.Set("sprite", pinkSprite);
        pinkSprite.Play("idle");
        Add(pinkSprite);
    }

    private static IDetour hook_WalkUpdate;

    internal static void LoadPatch() 
    {
        hook_WalkUpdate = new Hook(
            typeof(Slime).GetMethod("WalkUpdate", BindingFlags.NonPublic | BindingFlags.Instance),
            Slime_WalkUpdate_patch
        );
    }

    internal static void UnloadPatch() 
    {
        hook_WalkUpdate.Dispose();
    }

    private delegate int orig_Slime_WalkUpdate(Slime self);

    private static int Slime_WalkUpdate_patch(orig_Slime_WalkUpdate orig, Slime self) 
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
        if (shield)
            shield.Gain();
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
                target = ((float)Math.Sign(value) * 0.6f);
            }
        }
        this.Speed.X = Calc.Approach(this.Speed.X, target + jumpAround, 0.04f * Engine.TimeMult);
        this.Speed.Y = Calc.Approach(this.Speed.Y, 1.8f, 0.15f * Engine.TimeMult);
        return 1;
    }
}
