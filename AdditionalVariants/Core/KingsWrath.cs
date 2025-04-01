using FortRise;
using Microsoft.Xna.Framework;
using TowerFall;

namespace AdditionalVariants;

public static class KingsWrath 
{
    public static void Load()
    {
        On.TowerFall.Crown.Update += Update_ctor;
    }

    public static void Unload()
    {
        On.TowerFall.Crown.Update -= Update_ctor;
    }

    private static void Update_ctor(On.TowerFall.Crown.orig_Update orig, TowerFall.Crown self)
    {
        if (self.OwnerIndex >= 0 && VariantManager.GetCustomVariant("AdditionalVariants/KingsWrath")[self.OwnerIndex])
        {
            if (self.CheckBelow())
            {
                self.RemoveSelf();
                var chalicePad = new ChalicePad(self.Position - new Vector2(20, -34f), 40);
                var chalice = new Chalice(chalicePad);
                var wrathGhost = new ChaliceGhost(self.OwnerIndex, chalice);
                self.Level.Add(wrathGhost);
                return;
            }
        }
        orig(self);
    }
}