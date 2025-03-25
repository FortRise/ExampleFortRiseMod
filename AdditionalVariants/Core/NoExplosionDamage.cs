using FortRise;
using Microsoft.Xna.Framework;

namespace AdditionalVariants;

public static class NoExplosionDamage 
{
    public static void Load()
    {
        On.TowerFall.Player.Hurt_Explosion_Vector2 += Hurt_Explosion;
    }

    public static void Unload()
    {
        On.TowerFall.Player.Hurt_Explosion_Vector2 -= Hurt_Explosion;
    }

    private static void Hurt_Explosion(On.TowerFall.Player.orig_Hurt_Explosion_Vector2 orig, TowerFall.Player self, TowerFall.Explosion explosion, Vector2 normal)
    {
        if (VariantManager.GetCustomVariant("AdditionalVariants/NoExplosionDamage")[self.PlayerIndex])
        {
            return;
        }
        orig(self, explosion, normal);
    }
}