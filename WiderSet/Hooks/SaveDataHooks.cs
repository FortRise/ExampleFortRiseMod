using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class SaveDataHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(SaveData), nameof(SaveData.Verify)),
            transpiler: new HarmonyMethod(SaveData_Verify_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> SaveData_Verify_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(MoveType.After, [ILMatch.LdcI4(4)]);
        cursor.EmitDelegate((int count) => count + 4);

        return cursor.Generate();
    }
}