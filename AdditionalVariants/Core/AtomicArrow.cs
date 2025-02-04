using FortRise;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using TowerFall;

namespace AdditionalVariants;

public static class AtomicArrow 
{
    public static void Load() 
    {
        On.TowerFall.Arrow.HitWall += AtomicArrow_HitWall;
    }

    public static void Unload() 
    {
        On.TowerFall.Arrow.HitWall -= AtomicArrow_HitWall;
    }

    private static void AtomicArrow_HitWall(On.TowerFall.Arrow.orig_HitWall orig, TowerFall.Arrow self, TowerFall.Platform platform)
    {
        if (DynamicData.For(self).Get<bool>("squished"))
            return;
        orig(self, platform);
        if (VariantManager.GetCustomVariant("AtomicArrow")[self.PlayerIndex]) 
        {
            if (self is SuperBombArrow) 
            {
                Explode(self, true);
                return;
            }
            Explode(self, false);
        }
    }

    private static void Explode(Arrow arrow, bool super)
    {
        Vector2 vector = (arrow.Position + Calc.AngleToVector(arrow.Direction + 3.1415927f, 10f)).Floor();
        if (super) 
        {
            if (!Explosion.SpawnSuper(arrow.Level, vector, arrow.PlayerIndex, false))
            {
                Explosion.SpawnSuper(arrow.Level, arrow.Position, arrow.PlayerIndex, false);
            }
        }
        else 
        {
            if (!Explosion.Spawn(arrow.Level, vector, arrow.PlayerIndex, false, false, false))
            {
                Explosion.Spawn(arrow.Level, arrow.Position, arrow.PlayerIndex, false, false, false);
            }
        }

        arrow.RemoveSelf();
        Sounds.pu_bombArrowExplode.Play(arrow.X, 1f);
    }
}