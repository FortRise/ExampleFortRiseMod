using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using Microsoft.Xna.Framework;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class GifExporterHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredConstructor(typeof(GifExporter), [typeof(ReplayData), typeof(Action<bool>)]),
            transpiler: new HarmonyMethod(GifExporter_ctor_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(GifExporter), nameof(GifExporter.Render)),
            transpiler: new HarmonyMethod(GifExporter_Render_Transpiler)
        );

        harmony.Patch(
            AccessTools.EnumeratorMoveNext(
                AccessTools.DeclaredMethod(typeof(GifExporter), "ExportGIF")
            ),
            transpiler: new HarmonyMethod(GifExporter_ExportGif_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> GifExporter_Render_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(MoveType.After, [ILMatch.Ldfld("display"), ILMatch.Call("get_Zero")]);
        cursor.EmitDelegate((Vector2 displayPos) =>
        {
            if (!WiderSetModule.IsWide)
            {
                return displayPos;
            }

            return new Vector2(displayPos.X - 50, displayPos.Y);
        });

        cursor.GotoNext(MoveType.After, [ILMatch.Ldfld("displayOffset")]);
        cursor.EmitDelegate((Vector2 displayPos) =>
        {
            if (!WiderSetModule.IsWide)
            {
                return displayPos;
            }

            return new Vector2(displayPos.X - 50, displayPos.Y);
        });

        return cursor.Generate();
    }

    private static IEnumerable<CodeInstruction> GifExporter_ExportGif_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
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