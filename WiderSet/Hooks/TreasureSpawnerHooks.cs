using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using FortRise.Transpiler;
using FortRise;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class TreasureSpawnerHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(TreasureSpawner), nameof(TreasureSpawner.GetChestSpawnsForLevel)),
            transpiler: new HarmonyMethod(TreasureSpawner_GetChestSpawnsForLevel_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(TreasureSpawner), nameof(TreasureSpawner.GetPickupsForTreasureDraft)),
            transpiler: new HarmonyMethod(TreasureSpawner_GetPickupsForTreasureDraft_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> TreasureSpawner_GetPickupsForTreasureDraft_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(MoveType.After, [ILMatch.LdcI4(4)]);
        cursor.EmitDelegate((int playerCount) =>
        {
            if (WiderSetModule.IsWide)
            {
                return playerCount + 4;
            }

            return playerCount;
        });

        return cursor.Generate();
    }

    private static IEnumerable<CodeInstruction> TreasureSpawner_GetChestSpawnsForLevel_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.Encompass((x) =>
        {
            while (x.Next(MoveType.After, [ILMatch.LdcR4(160f)]))
            {
                cursor.EmitDelegate((float width) =>
                {
                    if (WiderSetModule.IsWide)
                    {
                        return width + 50;
                    }

                    return width;
                });
            }
        });

        return cursor.Generate();
    }
}