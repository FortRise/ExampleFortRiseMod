using FortRise;
using HarmonyLib;
using Microsoft.Xna.Framework;
using TowerFall;

namespace Teuria.AdditionalVariants;

public class KingsWrath : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Crown), nameof(Crown.Update)),
            new HarmonyMethod(Crown_Update_Prefix)
        );
    }

    private static bool Crown_Update_Prefix(TowerFall.Crown __instance)
    {
        if (__instance.OwnerIndex >= 0 && Variants.KingsWrath.IsActive(__instance.OwnerIndex))
        {
            if (__instance.CheckBelow())
            {
                __instance.RemoveSelf();
                var chalicePad = new ChalicePad(__instance.Position - new Vector2(20, -34f), 40);
                var chalice = new Chalice(chalicePad);
                var wrathGhost = new ChaliceGhost(__instance.OwnerIndex, chalice);
                __instance.Level.Add(wrathGhost);
                return false;
            }
        }

        return true;
    }
}