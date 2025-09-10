using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class SwitchBlockHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(SwitchBlock), "DrawSolid"),
            transpiler: new HarmonyMethod(SwitchBlock_DrawSolid_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> SwitchBlock_DrawSolid_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.Encompass(x =>
        {
            while (x.Next(MoveType.After, [ILMatch.LdcR4(320f)]))
            {
                cursor.EmitDelegate((float width) =>
                {
                    if (WiderSetModule.IsWide)
                    {
                        return width + 100;
                    }

                    return width;
                });
            }
        });

        cursor.Index = 0;

        cursor.Encompass(x =>
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
