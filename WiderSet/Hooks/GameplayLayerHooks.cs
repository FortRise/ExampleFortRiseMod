using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class GameplayLayerHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(GameplayLayer), nameof(GameplayLayer.BatchedRender)),
            transpiler: new HarmonyMethod(GameplayLayer_BatchedRender_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> GameplayLayer_BatchedRender_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.Encompass(x =>
        {
            while (x.Next(MoveType.After, [ILMatch.LdcR4(320f)]))
            {
                cursor.EmitDelegate((float x) => {
                    if (WiderSetModule.IsWide)
                    {
                        return x + 100f;
                    }
                    return x;
                });
            }
        });

        cursor.Index = 0;

        cursor.Encompass(x =>
        {
            while (x.Next(MoveType.After, [ILMatch.LdcR4(160f)]))
            {
                cursor.EmitDelegate((float x) => {
                    if (WiderSetModule.IsWide)
                    {
                        return x + 50f;
                    }
                    return x;
                });
            }
        });

        return cursor.Generate();
    }
}