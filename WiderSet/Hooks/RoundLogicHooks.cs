using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class RoundLogicHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredConstructor(typeof(RoundLogic), [typeof(Session), typeof(bool)]),
            transpiler: new HarmonyMethod(RoundLogic_ctor_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(RoundLogic), "SpawnPlayersFFA"),
            transpiler: new HarmonyMethod(RoundLogic_SpawnPlayersFFA_Teams_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(RoundLogic), "SpawnPlayersTeams"),
            transpiler: new HarmonyMethod(RoundLogic_SpawnPlayersFFA_Teams_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> RoundLogic_ctor_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(MoveType.After, [ILMatch.LdcI4(4)]);
        cursor.EmitDelegate((int player) =>
        {
            if (WiderSetModule.IsWide)
            {
                return player + 4;
            }

            return player;
        });

        return cursor.Generate();
    }

    private static IEnumerable<CodeInstruction> RoundLogic_SpawnPlayersFFA_Teams_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.Encompass(x =>
        {
            while (x.Next(MoveType.After, [ILMatch.LdcI4(4)]))
            {
                cursor.EmitDelegate((int playerCount) =>
                {
                    if (WiderSetModule.IsWide)
                    {
                        return playerCount + 4;
                    }

                    return playerCount;
                });
            }
        });

        cursor.Index = 0;

        cursor.GotoNext(MoveType.After, [ILMatch.LdcR4(160f)]);
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