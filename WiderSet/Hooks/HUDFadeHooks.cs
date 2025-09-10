using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using Monocle;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class HUDFadeHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(HUDFade), nameof(HUDFade.Render)),
            transpiler: new HarmonyMethod(HUDFade_Render_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> HUDFade_Render_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.LdcR4(0f)
            ]
        );

        cursor.EmitDelegate((float x) =>
        {
            if (WiderSetModule.IsWide)
            {
                return x - OffsetUtilities.OffsetX;
            }

            return x;
        });

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.LdcR4(320f)
            ]
        );

        cursor.EmitDelegate((float x) =>
        {
            if (WiderSetModule.IsWide)
            {
                return x + 100f;
            }

            return x;
        });

        return cursor.Generate();
    }
}
