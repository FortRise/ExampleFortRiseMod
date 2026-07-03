using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class GamepadConfigHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(GamepadConfig), nameof(GamepadConfig.GetDefaults)),
            transpiler: new HarmonyMethod(GamepadConfig_GetDefaults_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> GamepadConfig_GetDefaults_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(MoveType.After, [ILMatch.LdcI4(4)]);
        cursor.EmitDelegate((int count) => count + 4);

        return cursor.Generate();
    }
}
