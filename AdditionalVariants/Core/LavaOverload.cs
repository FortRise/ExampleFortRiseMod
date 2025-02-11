
using FortRise;
using Monocle;
using MonoMod.Utils;
using TowerFall;

namespace AdditionalVariants;

public static class LavaOverload 
{
    private static Action<LavaControl> base_Added;

    public static void Load()
    {
        base_Added = CallHelper.CallBaseGen<Entity, LavaControl>("Added");
        On.TowerFall.LavaControl.Added += LavaOverloadVariant;
    }

    public static void Unload()
    {
        On.TowerFall.LavaControl.Added -= LavaOverloadVariant;
    }

    private static void LavaOverloadVariant(On.TowerFall.LavaControl.orig_Added orig, TowerFall.LavaControl self)
    {
        if (VariantManager.GetCustomVariant("AdditionalVariants/LavaOverload")) 
        {
            base_Added(self);
            Sounds.sfx_lavaLoop.SetVolume(0f);
            Sounds.sfx_lavaLoop.Play(160f, 1f);
            var lavas = new Lava[4];
            self.Scene.Add(lavas[0] = new Lava(self, Lava.LavaSide.Left));
            self.Scene.Add(lavas[1] = new Lava(self, Lava.LavaSide.Right));
            self.Scene.Add(lavas[2] = new Lava(self, Lava.LavaSide.Top));
            self.Scene.Add(lavas[3] = new Lava(self, Lava.LavaSide.Bottom));
            DynamicData.For(self).Set("lavas", lavas);
            return;
        }
        orig(self);
    }
}