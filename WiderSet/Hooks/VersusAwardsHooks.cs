using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class VersusAwardsHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(VersusAwards), nameof(VersusAwards.GetAwards)),
            transpiler: new HarmonyMethod(VersusAwards_ExtendPlayers_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(VersusAwards), "AssignAward"),
            transpiler: new HarmonyMethod(VersusAwards_ExtendPlayers_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> VersusAwards_ExtendPlayers_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.Encompass((x) =>
        {
            while (x.Next(MoveType.After, [ILMatch.LdcI4(4)]))
            {
                cursor.EmitDelegate((int x) =>
                {
                    if (WiderSetModule.IsWide)
                    {
                        return x + 4;
                    }

                    return x;
                });
            }
        });

        return cursor.Generate();
    }
}
