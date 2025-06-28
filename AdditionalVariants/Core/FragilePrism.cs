using FortRise;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using TowerFall;

namespace Teuria.AdditionalVariants;

public class FragilePrism : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Prism), nameof(Prism.CaptiveForceShatter)),
            postfix: new HarmonyMethod(Prism_CaptiveForceShatter_Postfix)
        );
    }

    private static void Prism_CaptiveForceShatter_Postfix(Prism __instance)
    {
        if (__instance.EncasedPlayer is null)
        {
            return;
        }
        if (!Variants.FragilePrism.IsActive(__instance.EncasedPlayer.PlayerIndex))
        {
            return;
        }
        var dyn = DynamicData.For(__instance);
        var counter = dyn.Get<float>("counter");
        var sprite = dyn.Get<SpritePart<int>>("sprite")!;

        if (!dyn.Get<bool>("finished") && counter > 0)
        {
            dyn.Set("counter", counter - 20);
            sprite.Position = Calc.Random.Range(-Vector2.One, Vector2.One * 2f);
        }
    }
}
