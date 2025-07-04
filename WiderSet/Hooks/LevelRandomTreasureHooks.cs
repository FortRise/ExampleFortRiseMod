using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class LevelRandomTreasureHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(LevelRandomTreasure), nameof(LevelRandomTreasure.AddRandomTreasure)),
            transpiler: new HarmonyMethod(LevelRandomTreasure_AddRandomTreasure_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(LevelRandomTreasure), "BigTreasureSpotValue"),
            transpiler: new HarmonyMethod(LevelRandomTreasure_BigTreasureSpotValue_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> LevelRandomTreasure_AddRandomTreasure_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.Encompass(x =>
        {
            while (x.Next(MoveType.After, [ILMatch.LdcI4(16)]))
            {
                cursor.EmitDelegate((int x) =>
                {
                    if (WiderSetModule.IsWide)
                    {
                        return x + 5;
                    }
                    return x;
                });
            }
        });

        cursor.Index = 0;

        cursor.Encompass(x =>
        {
            while (x.Next(MoveType.After, [ILMatch.LdcR4(160)]))
            {
                cursor.EmitDelegate((float x) =>
                {
                    if (WiderSetModule.IsWide)
                    {
                        return x + 50;
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

        cursor.Encompass(x =>
        {
            while (x.Next(MoveType.After, [ILMatch.LdcR4(15)]))
            {
                cursor.EmitDelegate((float x) =>
                {
                    if (WiderSetModule.IsWide)
                    {
                        return x + 5;
                    }
                    return x;
                });
            }
        });

        cursor.Index = 0;

        cursor.Encompass(x =>
        {
            while (x.Next(MoveType.After, [ILMatch.LdcI4(14)]))
            {
                cursor.EmitDelegate((int x) =>
                {
                    if (WiderSetModule.IsWide)
                    {
                        return x + 5;
                    }
                    return x;
                });
            }
        });

        return cursor.Generate();
    }

    private static IEnumerable<CodeInstruction> LevelRandomTreasure_BigTreasureSpotValue_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(MoveType.After, [ILMatch.LdcR4(110f)]);

        cursor.EmitDelegate((float width) =>
        {
            if (WiderSetModule.IsWide)
            {
                return width + 50;
            }

            return width;
        });

        return cursor.Generate();
    }
}
