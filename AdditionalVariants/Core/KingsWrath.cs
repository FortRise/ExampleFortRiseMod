using Microsoft.Xna.Framework;
using TowerFall;

namespace Teuria.AdditionalVariants;

public class KingsWrath : IHookable
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
        if (self.OwnerIndex >= 0 && Variants.KingsWrath.IsActive(self.OwnerIndex))
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