using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using Microsoft.Xna.Framework;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class LightingLayerHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredConstructor(typeof(LightingLayer), [typeof(Color)]),
            transpiler: new HarmonyMethod(LightingLayer_ctor_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> LightingLayer_ctor_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.LdcI4(320)
            ]
        );

        cursor.EmitDelegate((int x) =>
        {
            if (WiderSetModule.IsWide)
            {
                return x + 100;
            }

            return x;
        });

        return cursor.Generate();
    }
}

internal sealed class MiasmaHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredConstructor(typeof(Miasma), [typeof(Miasma.Modes)]),
            transpiler: new HarmonyMethod(Miasma_ctor_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Miasma), nameof(Miasma.Update)),
            transpiler: new HarmonyMethod(Miasma_Update_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Miasma), nameof(Miasma.Render)),
            transpiler: new HarmonyMethod(Miasma_Render_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Miasma), nameof(Miasma.OnPlayerCollide)),
            transpiler: new HarmonyMethod(Miasma_OnPlayerCollide_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> Miasma_ctor_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

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

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.LdcR4(330f)
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

    private static IEnumerable<CodeInstruction> Miasma_Update_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.Encompass(x =>
        {
            while (x.Next(MoveType.After, [ILMatch.LdcR4(160f)]))
            {
                cursor.EmitDelegate((float x) =>
                {
                    if (WiderSetModule.IsWide)
                    {
                        return x + 50f;
                    }
                    return x;
                });
            }
        });

        cursor.Index = 0;

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

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.LdcR4(332f)
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

    private static IEnumerable<CodeInstruction> Miasma_Render_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.LdcR4(229f)
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

    private static IEnumerable<CodeInstruction> Miasma_OnPlayerCollide_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.LdcR4(160f)
            ]
        );

        cursor.EmitDelegate((float x) =>
        {
            if (WiderSetModule.IsWide)
            {
                return x + 50f;
            }

            return x;
        });

        return cursor.Generate();
    }
}