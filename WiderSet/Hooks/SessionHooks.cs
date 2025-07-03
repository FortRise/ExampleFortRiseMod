using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class SessionHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredConstructor(typeof(Session), [typeof(MatchSettings)]),
            transpiler: new HarmonyMethod(Session_ctor_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> Session_ctor_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
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

        return cursor.Generate();
    }
}