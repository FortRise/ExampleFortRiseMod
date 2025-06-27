using FortRise;
using HarmonyLib;
using TowerFall;

namespace Teuria.AdditionalVariants;

public class ExplodingShield : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredPropertySetter(
                typeof(Player),
                nameof(Player.HasShield)
            ),
            new HarmonyMethod(Player_set_HasShield_Prefix)
        );
    }

    private static void Player_set_HasShield_Prefix(Player __instance, bool value)
    {
        if (Variants.ExplodingShield.IsActive(__instance.PlayerIndex))
        {
            if (!value)
            {
                Explosion.Spawn(__instance.Level, __instance.Position, __instance.PlayerIndex, false, false, false);
                Sounds.pu_bombArrowExplode.Play(__instance.X, 1f);
            }
        }
    }
}