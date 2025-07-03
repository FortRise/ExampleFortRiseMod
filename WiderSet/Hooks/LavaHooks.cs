using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class LavaHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredConstructor(typeof(Lava), [typeof(LavaControl), typeof(Lava.LavaSide)]),
            transpiler: new HarmonyMethod(Lava_ctor_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Lava), nameof(Lava.Render)),
            transpiler: new HarmonyMethod(Lava_Render_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Lava), nameof(Lava.Update)),
            transpiler: new HarmonyMethod(Lava_Update_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Lava), nameof(Lava.DrawLight)),
            transpiler: new HarmonyMethod(Lava_DrawLight_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> Lava_DrawLight_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.Encompass((x) =>
        {
            while (x.Next(MoveType.After, [ILMatch.LdcR4(320f)]))
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

    private static IEnumerable<CodeInstruction> Lava_Update_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.Encompass((x) =>
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

        cursor.Index = 0;

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.LdcR4(305f)
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
                ILMatch.LdcR4(310f)
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

    private static IEnumerable<CodeInstruction> Lava_Render_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.Encompass((x) =>
        {
            while (x.Next(MoveType.After, [ILMatch.LdcR4(320f)]))
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

    private static IEnumerable<CodeInstruction> Lava_ctor_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.Encompass((x) =>
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

        cursor.Index = 0;

        cursor.Encompass((x) =>
        {
            while (x.Next(MoveType.After, [ILMatch.LdcR4(320f)]))
            {
                cursor.EmitDelegate((float x) =>
                {
                    if (WiderSetModule.IsWide)
                    {
                        return x + 100f;
                    }

                    return x;
                });
            }
        });

        cursor.Index = 0;

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.LdcR4(370f)
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
