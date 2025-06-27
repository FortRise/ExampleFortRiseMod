using TowerFall;
using FortRise;
using HarmonyLib;
using Microsoft.Xna.Framework;

namespace Teuria.AdditionalVariants;

public class NoExplosionDamage : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(
                typeof(Player),
                nameof(Player.Hurt),
                [typeof(Explosion), typeof(Vector2)]
            ),
            new HarmonyMethod(Player_Hurt_Prefix)
        );
    }

    private static bool Player_Hurt_Prefix(TowerFall.Player __instance)
    {
        if (Variants.NoExplosionDamage.IsActive(__instance.PlayerIndex))
        {
            return false;
        }

        return true;
    }
}