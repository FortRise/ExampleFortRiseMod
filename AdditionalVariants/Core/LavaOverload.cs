using System;
using FortRise;
using HarmonyLib;
using Monocle;
using MonoMod.Utils;
using TowerFall;

namespace Teuria.AdditionalVariants;


public class LavaOverload : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(LavaControl), nameof(LavaControl.Added)),
            new HarmonyMethod(LavaControl_Added_Prefix)
        );

        Harmony.ReversePatch(
            AccessTools.DeclaredMethod(typeof(Entity), nameof(Entity.Added)),
            new HarmonyMethod(Entity_Added_ReversePatch)
        );
    }

    private static void Entity_Added_ReversePatch(Entity original)
    {
        throw new NotImplementedException();
    }

    private static bool LavaControl_Added_Prefix(TowerFall.LavaControl __instance)
    {
        if (Variants.LavaOverload.IsActive())
        {
            Entity_Added_ReversePatch(__instance);
            Sounds.sfx_lavaLoop.SetVolume(0f);
            Sounds.sfx_lavaLoop.Play(160f, 1f);
            var lavas = new Lava[4];
            __instance.Scene.Add(lavas[0] = new Lava(__instance, Lava.LavaSide.Left));
            __instance.Scene.Add(lavas[1] = new Lava(__instance, Lava.LavaSide.Right));
            __instance.Scene.Add(lavas[2] = new Lava(__instance, Lava.LavaSide.Top));
            __instance.Scene.Add(lavas[3] = new Lava(__instance, Lava.LavaSide.Bottom));
            DynamicData.For(__instance).Set("lavas", lavas);
            return false;
        }

        return true;
    }
}