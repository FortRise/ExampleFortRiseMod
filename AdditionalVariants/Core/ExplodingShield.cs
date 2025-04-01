using System;
using FortRise;
using MonoMod.RuntimeDetour;
using TowerFall;

namespace AdditionalVariants;

public static class ExplodingShield
{
    private static Hook Player_set_HasShield = null!;
    public static void Load()
    {
        Player_set_HasShield = new Hook(
            typeof(Player).GetProperty("HasShield")!.GetSetMethod()!,
            set_HasShield_patch
        );
    }

    public static void Unload()
    {
        Player_set_HasShield.Dispose();
    }

    private static void set_HasShield_patch(Action<Player, bool> orig, Player self, bool value)
    {
        if (VariantManager.GetCustomVariant("AdditionalVariants/ExplodingShield")[self.PlayerIndex])
        {
            if (!value)
            {
                Explosion.Spawn(self.Level, self.Position, self.PlayerIndex, false, false, false);
                Sounds.pu_bombArrowExplode.Play(self.X, 1f);
            }
        }

        orig(self, value);
    }
}