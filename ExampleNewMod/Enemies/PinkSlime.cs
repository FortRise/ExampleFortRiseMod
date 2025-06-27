using System;
using System.Collections.Generic;
using FortRise;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using TowerFall;

namespace ExampleNewMod;

public class PinkSlime : Slime
{
    private static ISpriteContainerEntry SlimeSprite { get; set; } = null!;
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

        var pinkSprite = ((ISpriteEntry<string>)SlimeSprite.Entry).Sprite;
        pinkSprite!.OnAnimationComplete = s =>
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

    public static void Register(IHarmony harmony, IModContent content, IModRegistry registry)
    {
        var pinkSlimeSprite = registry.Subtextures.RegisterTexture(
            content.Root.GetRelativePath("Content/Atlas/atlas/pinkSlime.png")
        );

        SlimeSprite = registry.Sprites.RegisterSprite<string>(
            "PinkSlime",
            new()
            {
                Texture = pinkSlimeSprite,
                FrameWidth = 20,
                FrameHeight = 20,
                OriginX = 10,
                OriginY = 20,
                AdditionalData = new Dictionary<string, object>()
                {
                    ["DownY"] = 1
                },
                Animations = [
                    new() { ID = "idle", Frames = [12], Loop = true },
                    new() { ID = "fall", Frames = [13, 14, 15, 16], Delay = 0.1f, Loop = true },
                    new() { ID = "land", Frames = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11], Delay = 0.06f, Loop = false },
                    new() { ID = "death", Frames = [18, 19, 20, 21, 22], Delay = 0.1f, Loop = false }
                ]
            }
        );

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

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Slime), "WalkUpdate"),
            new HarmonyMethod(Slime_WalkUpdate_patch)
        );
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


    private static bool Slime_WalkUpdate_patch(Slime __instance, ref int __result)
    {
        if (__instance is PinkSlime pink)
        {
            if (pink.ShouldTurnAround())
            {
                pink.jumpAround = 0.2f * (int)pink.Facing;
                __result = 2;
                return false;
            }
        }
        return true;
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

