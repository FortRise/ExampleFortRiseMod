using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class LevelRandomBGTilesHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(LevelRandomBGTiles), "Set"),
            transpiler: new HarmonyMethod(LevelRandomBGTiles_Set_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(LevelRandomBGTiles), nameof(LevelRandomBGTiles.GenerateBitData)),
            transpiler: new HarmonyMethod(LevelRandomBGTiles_GenerateBitData_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> LevelRandomBGTiles_GenerateBitData_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
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
            while (x.Next(MoveType.After, [ILMatch.LdcI4(31)]))
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

        return cursor.Generate();
    }

    private static IEnumerable<CodeInstruction> LevelRandomBGTiles_Set_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
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
}
