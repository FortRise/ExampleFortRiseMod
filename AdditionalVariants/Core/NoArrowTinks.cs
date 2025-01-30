using FortRise;
using MonoMod.Cil;
using TowerFall;

namespace AdditionalVariants;

public static class NoArrowTinks 
{
    public static void Load()
    {
        IL.TowerFall.Arrow.Update += Update_ctor;
    }

    public static void Unload()
    {
        IL.TowerFall.Arrow.Update -= Update_ctor;
    }

    private static void Update_ctor(ILContext il)
    {
        var cursor = new ILCursor(il);

        if (cursor.TryGotoNext(MoveType.After, 
            instr => instr.MatchLdloc(2),
            instr => instr.MatchCallOrCallvirt<Arrow>("get_Dangerous")))
        {
            cursor.EmitDelegate((bool isDangerous) => isDangerous && !VariantManager.GetCustomVariant("NoArrowTinks"));
        }
    }
}