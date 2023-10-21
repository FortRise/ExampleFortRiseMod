using FortRise;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using TowerFall;

namespace AdditionalVariants;

public static class FadingArrow 
{
    public static void Load() 
    {
        On.TowerFall.Arrow.Update += Update_patch;
        On.TowerFall.Arrow.Create += Create_patch;
        IL.TowerFall.Arrow.Update += InlineUpdate_patch;
    }

    public static void Unload() 
    {
        On.TowerFall.Arrow.Update -= Update_patch;
        On.TowerFall.Arrow.Create -= Create_patch;
        IL.TowerFall.Arrow.Update -= InlineUpdate_patch;
    }

    private static Arrow Create_patch(On.TowerFall.Arrow.orig_Create orig, ArrowTypes type, LevelEntity owner, Vector2 position, float direction, int? overrideCharacterIndex, int? overridePlayerIndex)
    {
        var arrow = orig(type, owner, position, direction, overrideCharacterIndex, overridePlayerIndex);
        arrow.StopFlashing();
        return arrow;
    }

    private static void InlineUpdate_patch(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);

        if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCall<TowerFall.LevelEntity>("get_Flashing"))) 
        {
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<bool, Arrow, bool>>((flashing, self) => {
                if (VariantManager.GetCustomVariant("FadingArrow")[self.PlayerIndex])
                    return false;
                return flashing;
            });
        }
    }

    private static void Update_patch(On.TowerFall.Arrow.orig_Update orig, TowerFall.Arrow self)
    {
        orig(self);
        if (self is LaserArrow || !VariantManager.GetCustomVariant("FadingArrow")[self.PlayerIndex]) 
            return;
        
        if (!self.Flashing && self.State >= TowerFall.Arrow.ArrowStates.Stuck) 
        {
            self.Flash(60, () => {
                self.StopFlashing();
                self.RemoveSelf();
            });
        }
    }
}