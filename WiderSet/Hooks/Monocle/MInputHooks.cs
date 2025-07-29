using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using Monocle;

namespace Teuria.WiderSet;

internal sealed class MInputHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(MInput), "UpdateJoysticks"),
            transpiler: new HarmonyMethod(MInput_UpdateJoysticks_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> MInput_UpdateJoysticks_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.Encompass(x =>
        {
            while (x.Next(MoveType.After, [ILMatch.LdcI4(4)]))
            {
                cursor.EmitDelegate((int player) => player + 4);
            }
        });

        return cursor.Generate();
    }
}