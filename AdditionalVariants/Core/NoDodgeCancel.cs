using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using TowerFall;

namespace Teuria.AdditionalVariants;

public class NoDodgeCancel : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Player), "DodgingUpdate"),
            transpiler: new HarmonyMethod(Player_DodgingUpdate_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> Player_DodgingUpdate_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.Ldfld("JumpPressed")
            ]
        );

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate((bool jumpPressed, Player self) =>
        {
            if (Variants.NoDodgeCancel.IsActive(self.PlayerIndex))
            {
                return false;
            }

            return jumpPressed;
        });

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.Ldfld("jumpBufferCounter"),
                ILMatch.Call("op_Implicit")
            ]
        );

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate((bool dodgePressed, Player self) =>
        {
            if (Variants.NoDodgeCancel.IsActive(self.PlayerIndex))
            {
                return false;
            }

            return dodgePressed;
        });

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.LdcR4(2.8f)
            ]
        );

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate((float speed, Player self) =>
        {
            var canActive = Variants.NoHypers.IsActive(self.PlayerIndex);
            if (canActive)
                return 1f;
            return speed;
        });

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.Ldfld("DodgePressed")
            ]
        );

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate((bool dodgePressed, Player self) =>
        {
            var canActive = Variants.NoDodgeCancel.IsActive(self.PlayerIndex) ||
                Variants.NoHypers.IsActive(self.PlayerIndex);
            if (canActive)
                return false;

            return dodgePressed;
        });

        return cursor.Generate();
    }
}