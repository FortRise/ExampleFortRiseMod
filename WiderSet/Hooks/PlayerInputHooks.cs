using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using TowerFall;

namespace Teuria.WiderSet;

public sealed class PlayerInputHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(PlayerInput), nameof(PlayerInput.AssignInputs)),
            transpiler: new HarmonyMethod(PlayerInput_AssignInputs_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> PlayerInput_AssignInputs_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.Encompass(x =>
        {
            while (x.Next(
                MoveType.After,
                [
                    ILMatch.LdcI4(4)
                ]
            )
            )
            {
                cursor.EmitDelegate((int x) => x + 4);
            }
        });

        cursor.Index = 0;

        cursor.Encompass(x =>
        {
            while (x.Next(
                MoveType.After,
                [
                    ILMatch.LdcI4(3)
                ]
            )
            )
            {
                cursor.EmitDelegate((int x) => x + 4);
            }
        });

        return cursor.Generate();
    }
}