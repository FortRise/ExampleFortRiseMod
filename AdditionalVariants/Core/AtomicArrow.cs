using FortRise;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using TowerFall;

namespace Teuria.AdditionalVariants;

public class AtomicArrow : IHookable
{
    public static void Load(IHarmony harmony) 
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Arrow), "HitWall"),
            postfix: new HarmonyMethod(Arrow_HitWall_Postfix)
        );
    }

    private static void Arrow_HitWall_Postfix(Arrow __instance)
    {
        if (DynamicData.For(__instance).Get<bool>("squished"))
        {
            return;
        }

        if (Variants.AtomicArrow.IsActive(__instance.PlayerIndex))
        {
            if (__instance is SuperBombArrow) 
            {
                Explode(__instance, true);
                return;
            }
            Explode(__instance, false);
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