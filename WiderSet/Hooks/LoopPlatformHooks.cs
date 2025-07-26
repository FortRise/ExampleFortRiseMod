using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using Microsoft.Xna.Framework;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class LoopPlatformHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredConstructor(typeof(LoopPlatform), [typeof(Vector2), typeof(int), typeof(LoopPlatform.MoveDirs), typeof(bool)]),
            transpiler: new HarmonyMethod(LoopPlatform_ctor_Vector2_int_LoopPlatform_MoveDirs_bool_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredConstructor(typeof(LoopPlatform), [typeof(Vector2), typeof(LoopPlatform)]),
            transpiler: new HarmonyMethod(LoopPlatform_ctor_Vector2_LoopPlatform_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(LoopPlatform), nameof(LoopPlatform.Added)),
            transpiler: new HarmonyMethod(LoopPlatform_Added_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(LoopPlatform), nameof(LoopPlatform.Update)),
            transpiler: new HarmonyMethod(LoopPlatform_Update_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(LoopPlatform), nameof(LoopPlatform.DrawLight)),
            transpiler: new HarmonyMethod(LoopPlatform_DrawLight_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> LoopPlatform_ctor_Vector2_int_LoopPlatform_MoveDirs_bool_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.Encompass(x =>
        {
            while (x.Next(MoveType.After, [ILMatch.LdcR4(320)]))
            {
                cursor.EmitDelegate((float x) =>
                {
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

    private static IEnumerable<CodeInstruction> LoopPlatform_ctor_Vector2_LoopPlatform_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.Encompass(x =>
        {
            while (x.Next(MoveType.After, [ILMatch.LdcI4(320)]))
            {
                cursor.EmitDelegate((int x) =>
                {
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

    private static IEnumerable<CodeInstruction> LoopPlatform_Added_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.Encompass(x =>
        {
            while (x.Next(MoveType.After, [ILMatch.LdcI4(32)]))
            {
                cursor.EmitDelegate((int x) =>
                {
                    if (WiderSetModule.IsWide)
                    {
                        return x + 10;
                    }

                    return x;
                });
            }
        });

        cursor.Index = 0;

        cursor.Encompass(x =>
        {
            while (x.Next(MoveType.After, [ILMatch.LdcR4(320)]))
            {
                cursor.EmitDelegate((float x) =>
                {
                    if (WiderSetModule.IsWide)
                    {
                        return x + 100;
                    }

                    return x;
                });
            }
        });

        cursor.Index = 0;

        cursor.GotoNext(MoveType.After, [ILMatch.LdcI4(31)]);
        cursor.EmitDelegate((int x) =>
        {
            if (WiderSetModule.IsWide)
            {
                return x + 10;
            }

            return x;
        });

        return cursor.Generate();
    }

    private static IEnumerable<CodeInstruction> LoopPlatform_Update_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);
        cursor.Encompass(x =>
        {
            while (x.Next(MoveType.After, [ILMatch.LdcR4(320)]))
            {
                cursor.EmitDelegate((float x) =>
                {
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

    private static IEnumerable<CodeInstruction> LoopPlatform_DrawLight_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(MoveType.After, [ILMatch.LdcR4(-320)]);
        cursor.EmitDelegate((float x) =>
        {
            if (WiderSetModule.IsWide)
            {
                return x - 100;
            }

            return x;
        });

        cursor.GotoNext(MoveType.After, [ILMatch.LdcR4(320)]);
        cursor.EmitDelegate((float x) =>
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