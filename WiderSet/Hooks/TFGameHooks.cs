using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using TowerFall;

namespace Teuria.WiderSet;

public sealed class TFGameHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(TFGame), "orig_Initialize"),
            transpiler: new HarmonyMethod(PlayerAmountUtilities.EightPlayerTranspilerNoCondition)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(TFGame), nameof(TFGame.CharacterTaken)),
            transpiler: new HarmonyMethod(PlayerAmountUtilities.EightPlayerTranspilerNoCondition)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(TFGame), nameof(TFGame.orig_ctor)),
            transpiler: new HarmonyMethod(TFGame_ctor_Transpiler)
        );
    }
    

    private static IEnumerable<CodeInstruction> TFGame_ctor_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.LdcI4(320)
            ]
        );

        cursor.EmitDelegate((int width) => width + 100);

        return cursor.Generate();
    }
}
