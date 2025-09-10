using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class PauseMenuHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(PauseMenu), nameof(PauseMenu.Render)),
            transpiler: new HarmonyMethod(PauseMenu_Render_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> PauseMenu_Render_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(MoveType.After, [ILMatch.LdcR4(0)]);
        cursor.EmitDelegate((float width) =>
        {
            if (WiderSetModule.IsWide)
            {
                return width - 50;
            }

            return width;
        });

        cursor.GotoNext(MoveType.After, [ILMatch.LdcR4(320f)]);
        cursor.EmitDelegate((float width) =>
        {
            if (WiderSetModule.IsWide)
            {
                return width + 100;
            }

            return width;
        });

        return cursor.Generate();
    }
}

