using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class LevelEntityHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(LevelEntity), "EnforceScreenWrap"),
            transpiler: new HarmonyMethod(LevelEntity_EnforceScreenWrap_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(LevelEntity), nameof(LevelEntity.Render)),
            transpiler: new HarmonyMethod(LevelEntity_Render_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> LevelEntity_EnforceScreenWrap_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
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

        return cursor.Generate();
    }

    private static IEnumerable<CodeInstruction> LevelEntity_Render_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
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

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.LdcR4(160f)
            ]
        );

        cursor.EmitDelegate((float x) => {
            if (WiderSetModule.IsWide)
            {
                return x + 50f;
            }
            return x;
        });

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.LdcI4(320)
            ]
        );

        cursor.EmitDelegate((int x) => {
            if (WiderSetModule.IsWide)
            {
                return x + 100;
            }
            return x;
        });


        return cursor.Generate();
    }
}
