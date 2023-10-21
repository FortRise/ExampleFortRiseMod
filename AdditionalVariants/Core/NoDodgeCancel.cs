using FortRise;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using TowerFall;

namespace AdditionalVariants;

public static class NoDodgeCancel 
{
    public static void Load() 
    {
        IL.TowerFall.Player.DodgingUpdate += DodgingUpdate_patch;
    }

    public static void Unload() 
    {
        IL.TowerFall.Player.DodgingUpdate -= DodgingUpdate_patch;
    }

    private static void DodgingUpdate_patch(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);

        if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<TowerFall.InputState>("JumpPressed"))) 
        {
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<bool, Player, bool>>((jumpPressed, self) => {
                if (VariantManager.GetCustomVariant("NoDodgeCancel")[self.PlayerIndex]) 
                    return false;
                
                return jumpPressed;
            });
        }

        if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCall<Monocle.Counter>("op_Implicit"))) 
        {
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<bool, Player, bool>>((dodgePressed, self) => {
                if (VariantManager.GetCustomVariant("NoDodgeCancel")[self.PlayerIndex]) 
                    return false;
                
                return dodgePressed;
            });
        }

        if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<TowerFall.InputState>("DodgePressed"))) 
        {
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<bool, Player, bool>>((dodgePressed, self) => {
                if (VariantManager.GetCustomVariant("NoDodgeCancel")[self.PlayerIndex]) 
                    return false;
                
                return dodgePressed;
            });
        }
    }
}