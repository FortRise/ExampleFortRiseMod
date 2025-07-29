using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using Monocle;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class VersusRoundResultsHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.EnumeratorMoveNext(
                AccessTools.DeclaredMethod(typeof(VersusRoundResults), "Sequence")
            ),
            transpiler: new HarmonyMethod(VersusRoundResults_ctor_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> VersusRoundResults_ctor_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(MoveType.After, [ILMatch.LdcI4(3)]);
        cursor.EmitDelegate((int player) =>
        {
            if (WiderSetModule.IsWide)
            {
                return player + 4;
            }

            return player;
        });

        return cursor.Generate();
    }
}
