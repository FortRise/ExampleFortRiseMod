using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using Microsoft.Xna.Framework;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class WrapMathHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(WrapMath), nameof(WrapMath.Opposite)),
            transpiler: new HarmonyMethod(WrapMath_Opposite_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(WrapMath), nameof(WrapMath.ApplyWrap), [typeof(Rectangle)]),
            transpiler: new HarmonyMethod(WrapMath_ApplyWrap_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(WrapMath), nameof(WrapMath.ApplyWrapX)),
            transpiler: new HarmonyMethod(WrapMath_CommonWrapFunc_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(WrapMath), nameof(WrapMath.ShortestOpen)),
            transpiler: new HarmonyMethod(WrapMath_CommonWrapFunc_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(WrapMath), nameof(WrapMath.DiffX)),
            transpiler: new HarmonyMethod(WrapMath_CommonWrapFunc_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(WrapMath), nameof(WrapMath.WrapHorizDistanceSquared)),
            transpiler: new HarmonyMethod(WrapMath_CommonWrapFunc_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(WrapMath), nameof(WrapMath.WrapHorizDistance)),
            transpiler: new HarmonyMethod(WrapMath_CommonWrapFunc_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(WrapMath), nameof(WrapMath.WrapHorizLineHit)),
            transpiler: new HarmonyMethod(WrapMath_CommonWrapFunc_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(WrapMath), nameof(WrapMath.WrapHorizAngle)),
            transpiler: new HarmonyMethod(WrapMath_CommonWrapFunc_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> WrapMath_Opposite_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.Encompass(x =>
        {
            while (x.Next(MoveType.After, [ILMatch.LdcR4(160f)]))
            {
                cursor.EmitDelegate((float x) => {
                    if (WiderSetModule.IsWide)
                    {
                        return x + 50f;
                    }
                    return x;
                });
            }
        });

        return cursor.Generate();
    }

    private static IEnumerable<CodeInstruction> WrapMath_ApplyWrap_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.Encompass(x =>
        {
            while (x.Next(MoveType.After, [ILMatch.LdcI4(320)]))
            {
                cursor.EmitDelegate((int x) => {
                    if (WiderSetModule.IsWide)
                    {
                        return x + 100;
                    }
                    return x;
                });
            }
        });

        return cursor.Generate();
    }

    private static IEnumerable<CodeInstruction> WrapMath_CommonWrapFunc_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.Encompass(x =>
        {
            while (x.Next(MoveType.After, [ILMatch.LdcR4(320f)]))
            {
                cursor.EmitDelegate((float x) => {
                    if (WiderSetModule.IsWide)
                    {
                        return x + 100;
                    }
                    return x;
                });
            }
        });

        return cursor.Generate();
    }
}
