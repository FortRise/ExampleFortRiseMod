using System;
using FortRise;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using TowerFall;

namespace Teuria.AdditionalVariants;

public static class ArrowFallingUp 
{
    public static void Load() 
    {
        IL.TowerFall.Arrow.ArrowUpdate += ArrowUpdate_patch;
        IL.TowerFall.Arrow.EnterFallMode += EnterFallMode_patch;
    }

    public static void Unload() 
    {
        IL.TowerFall.Arrow.ArrowUpdate -= ArrowUpdate_patch;
        IL.TowerFall.Arrow.EnterFallMode -= EnterFallMode_patch;
    }

    private static void EnterFallMode_patch(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);

        if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(-2))) 
        {
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<int, Arrow, int>>((gravity, self) => {
                if (VariantManager.GetCustomVariant("AdditionalVariants/ArrowFallingUp")[self.PlayerIndex]) 
                {
                    return -gravity;
                }
                return gravity;
            });
        }
    }

    private static void ArrowUpdate_patch(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);

        if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(0.2f)))
        {
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<float, Arrow, float>>((gravity, self) => {
                if (VariantManager.GetCustomVariant("AdditionalVariants/ArrowFallingUp")[self.PlayerIndex]) 
                {
                    return -gravity;
                }
                return gravity;
            });
        }

        if (cursor.TryGotoNext(MoveType.Before, instr => instr.MatchClt()))
        {
            cursor.Next!.OpCode = OpCodes.Cgt;
        }
        // TODO, makes clt, cge
        if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdloc(1)))
        {
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<int, Arrow, int>>((gravity, self) => {
                if (VariantManager.GetCustomVariant("AdditionalVariants/ArrowFallingUp")[self.PlayerIndex]) 
                {
                    return -gravity;
                }
                return gravity;
            });
        }
        if (cursor.TryGotoPrev(MoveType.After, instr => instr.MatchLdcR4(1f))) 
        {
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<float, Arrow, float>>((gravity, self) => {
                if (VariantManager.GetCustomVariant("AdditionalVariants/ArrowFallingUp")[self.PlayerIndex]) 
                {
                    return -gravity;
                }
                return gravity;
            });
            // cursor.Next.OpCode = OpCodes.Bge;
        }
        if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(-0.06981317f))) 
        {
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<float, Arrow, float>>((gravity, self) => {
                if (VariantManager.GetCustomVariant("AdditionalVariants/ArrowFallingUp")[self.PlayerIndex]) 
                {
                    return -3.07178f;
                }
                return gravity;
            });
        }

        if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(0.06981317f))) 
        {
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<float, Arrow, float>>((gravity, self) => {
                if (VariantManager.GetCustomVariant("AdditionalVariants/ArrowFallingUp")[self.PlayerIndex]) 
                {
                    return 3.07178f;
                }
                return gravity;
            });
        }
    }
}