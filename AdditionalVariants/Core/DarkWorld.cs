using FortRise;
using HarmonyLib;
using MonoMod.Utils;
using TowerFall;

namespace Teuria.AdditionalVariants;

public class DarkWorld : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Background), nameof(Background.Render)),
            new HarmonyMethod(Background_Render_Prefix)
        );
    }

    private static bool Background_Render_Prefix(TowerFall.Background __instance)
    {
        if (Variants.DarkWorld.IsActive())
        {
            var level = DynamicData.For(__instance).Get<Level>("level")!;
            var darkened = DynamicData.For(level.OrbLogic).Get<bool>("darkened");
            if (darkened)
            {
                return false;
            }
        }
        return true;
    }
}