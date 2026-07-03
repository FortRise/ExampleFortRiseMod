using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class MenuInputHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(MenuInput), nameof(MenuInput.UpdateInputs)),
            transpiler: new HarmonyMethod(MenuInput_UpdateInputs_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> MenuInput_UpdateInputs_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(MoveType.After, [ILMatch.LdcI4(5)]);

        cursor.EmitDelegate((int player) =>
        {
            if (WiderSetModule.IsWide)
            {
                return player + 4;
            }

            return player;
        });

        cursor.Encompass(x =>
        {
            while (x.Next(MoveType.After, [ILMatch.LdcI4(4)]))
            {
                cursor.EmitDelegate((int player) =>
                {
                    if (WiderSetModule.IsWide)
                    {
                        return player + 4;
                    }

                    return player;
                });
            }
        });

        return cursor.Generate();
    }
}
