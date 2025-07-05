using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using Moments.Encoder;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class GifEncoderHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(GifEncoder), nameof(GifEncoder.AddFrame)),
            transpiler: new HarmonyMethod(GifEncoder_AddFrame_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(GifEncoder), "GetImagePixels"),
            transpiler: new HarmonyMethod(GifEncoder_GetImagePixels_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> GifEncoder_AddFrame_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(MoveType.After, [ILMatch.LdcI4(320)]);

        cursor.EmitDelegate((int width) =>
        {
            return width + 100;
        });

        return cursor.Generate();
    }

    private static IEnumerable<CodeInstruction> GifEncoder_GetImagePixels_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(MoveType.After, [ILMatch.LdcI4(230400)]);

        cursor.EmitDelegate((int pixels) =>
        {
            if (WiderSetModule.IsWide)
            {
                return 420 * 240 * 3;
            }

            return pixels;
        });

        cursor.GotoNext(MoveType.After, [ILMatch.LdcI4(76800)]);

        cursor.EmitDelegate((int pixels) =>
        {
            if (WiderSetModule.IsWide)
            {
                return 420 * 240;
            }

            return pixels;
        });

        cursor.Encompass((x) =>
        {
            while (x.Next(MoveType.After, [ILMatch.LdcI4(320)]))
            {
                cursor.EmitDelegate((int width) =>
                {
                    if (WiderSetModule.IsWide)
                    {
                        return width + 100;
                    }

                    return width;
                });
            }
        });

        return cursor.Generate();
    }
}

internal sealed class GifExporterHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredConstructor(typeof(GifExporter), [typeof(ReplayData), typeof(Action<bool>)]),
            transpiler: new HarmonyMethod(GifExporter_ctor_Transpiler)
        );

        harmony.Patch(
            AccessTools.EnumeratorMoveNext(
                AccessTools.DeclaredMethod(typeof(GifExporter), "ExportGIF")
            ),
            transpiler: new HarmonyMethod(GifExporter_Render_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> GifExporter_Render_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(MoveType.After, [ILMatch.LdcI4(320)]);

        cursor.EmitDelegate((int width) =>
        {
            if (WiderSetModule.IsWide)
            {
                return width + 100;
            }

            return width;
        });

        return cursor.Generate();
    }

    private static IEnumerable<CodeInstruction> GifExporter_ctor_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(MoveType.After, [ILMatch.LdcI4(320)]);

        cursor.EmitDelegate((int width) =>
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