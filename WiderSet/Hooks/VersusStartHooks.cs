using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using Monocle;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class VersusStartHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(VersusStart), nameof(VersusStart.Render)),
            transpiler: new HarmonyMethod(VersusStart_Render_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> VersusStart_Render_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(
            MoveType.After,
            [
                WiderSetModule.Instance.Context.Flags.IsWindows ? ILMatchExt.Ble_Un_S() : ILMatch.Brtrue_S(),
                ILMatch.LdcR4(0f)
            ]
        );

        cursor.EmitDelegate((float x) =>
        {
            if (WiderSetModule.IsWide)
            {
                return x - Screen.LeftImage.Width - 4;
            }

            return x - 4;
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
