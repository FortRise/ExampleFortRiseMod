using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class LevelRandomBGDetailsHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(LevelRandomBGDetails), nameof(LevelRandomBGDetails.GenCataclysm)),
            transpiler: new HarmonyMethod(LevelRandomBGDetails_GenCataclysm_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(LevelRandomBGDetails), nameof(LevelRandomBGDetails.Check)),
            transpiler: new HarmonyMethod(LevelRandomBGDetails_Check_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(LevelRandomBGDetails), nameof(LevelRandomBGDetails.Empty)),
            transpiler: new HarmonyMethod(LevelRandomBGDetails_Empty_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> LevelRandomBGDetails_Empty_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

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

    private static IEnumerable<CodeInstruction> LevelRandomBGDetails_Check_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(MoveType.After, [ILMatch.LdcI4(32)]);

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

    private static IEnumerable<CodeInstruction> LevelRandomBGDetails_GenCataclysm_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.Encompass(x =>
        {
            int i = 0;
            while (x.Next(MoveType.After, [ILMatch.LdcI4(16)]))
            {
                if (i is 13 or 14 or 15)
                {
                    i += 1;
                    continue;
                }

                cursor.EmitDelegate((int x) =>
                {
                    if (WiderSetModule.IsWide)
                    {
                        return x + 5;
                    }
                    return x;
                });

                i += 1;
            }
        });

        cursor.Index = 0;

        cursor.Encompass(x =>
        {
            int i = 0;
            while (x.Next(MoveType.After, [ILMatch.LdcI4(32)]))
            {
                if (i is 5 or 6)
                {
                    i += 1;
                    continue;
                }
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
            int i = 0;
            while (x.Next(MoveType.After, [ILMatch.LdcI4(31)]))
            {
                if (i is 0 or 1)
                {
                    i += 1;
                    continue;
                }
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

        return cursor.Generate();
    }
}
