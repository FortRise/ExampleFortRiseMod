using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class LevelLoaderXMLHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.EnumeratorMoveNext(
                AccessTools.DeclaredMethod(typeof(LevelLoaderXML), "Load")
            ),
            transpiler: new HarmonyMethod(LevelLoaderXML_Load_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> LevelLoaderXML_Load_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.Encompass(x =>
        {
            while (x.Next(MoveType.After, [ILMatch.LdcI4(32)]))
            {
                cursor.EmitDelegate((int width) =>
                {
                    if (WiderSetModule.IsWide)
                    {
                        return width + 10;
                    }

                    return width;
                });
            }
        });

        return cursor.Generate();
    }
}