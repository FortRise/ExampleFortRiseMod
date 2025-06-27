using TowerFall;
using FortRise;
using HarmonyLib;

namespace Teuria.AdditionalVariants;

public class BottomlessQuiver : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(
                typeof(Player),
                nameof(Player.Added)
            ),
            postfix: new HarmonyMethod(Player_Added_Postfix)
        );
    }

    private static void Player_Added_Postfix(TowerFall.Player __instance)
    {
        if (Variants.BottomlessQuiver.IsActive(__instance.PlayerIndex))
        {
            __instance.Arrows.SetMaxArrows(int.MaxValue);
        }
    }
}
