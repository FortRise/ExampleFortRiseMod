using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class SavingInfoSceneHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(SavingInfoScene), nameof(SavingInfoScene.Render)),
            transpiler: new HarmonyMethod(SavingInfoScene_Render_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> SavingInfoScene_Render_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(MoveType.After, [ILMatch.LdcR4(160)]);
        cursor.EmitDelegate((float width) => width + 50);
        cursor.GotoNext(MoveType.After, [ILMatch.LdcR4(160)]);
        cursor.EmitDelegate((float width) => width + 50);
        cursor.GotoNext(MoveType.After, [ILMatch.LdcR4(160)]);
        cursor.EmitDelegate((float width) => width + 50);
        cursor.GotoNext(MoveType.After, [ILMatch.LdcR4(160)]);
        cursor.EmitDelegate((float width) => width + 50);

        cursor.Index = 0;

        cursor.GotoNext(MoveType.After, [ILMatch.LdcR4(320f)]);
        cursor.EmitDelegate((float width) => width + 100);

        return cursor.Generate();
    }
}


