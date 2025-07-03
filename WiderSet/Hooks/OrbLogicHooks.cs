using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class OrbLogicHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(OrbLogic), nameof(OrbLogic.CancelVariants)),
            transpiler: new HarmonyMethod(OrbLogic_CancelVariants_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(OrbLogic), nameof(OrbLogic.DoSpaceOrb)),
            transpiler: new HarmonyMethod(OrbLogic_DoSpaceOrb_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(OrbLogic), nameof(OrbLogic.DoSpaceOrb)),
            transpiler: new HarmonyMethod(OrbLogic_DoOffsetWorldVariant_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(OrbLogic), nameof(OrbLogic.EndScroll)),
            transpiler: new HarmonyMethod(OrbLogic_EndScroll_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> OrbLogic_DoOffsetWorldVariant_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.Encompass(x =>
        {
            while (x.Next(MoveType.After, [ILMatch.LdcR4(160f)]))
            {
                cursor.EmitDelegate((float x) => {
                    if (WiderSetModule.IsWide)
                    {
                        return x + 50;
                    }
                    return x;
                });
            }
        });

        cursor.Encompass(x =>
        {
            while (x.Next(MoveType.After, [ILMatch.LdcR4(-160f)]))
            {
                cursor.EmitDelegate((float x) => {
                    if (WiderSetModule.IsWide)
                    {
                        return x - 50;
                    }
                    return x;
                });
            }
        });

        return cursor.Generate();
    }

    private static IEnumerable<CodeInstruction> OrbLogic_EndScroll_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.LdcR4(320)
            ]
        );

        cursor.EmitDelegate((float x) => {
            if (WiderSetModule.IsWide)
            {
                return x + 100;
            }
            return x;
        });

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.LdcI4(160)
            ]
        );

        cursor.EmitDelegate((int x) => {
            if (WiderSetModule.IsWide)
            {
                return x + 50;
            }
            return x;
        });


        return cursor.Generate();
    }

    private static IEnumerable<CodeInstruction> OrbLogic_DoSpaceOrb_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.Encompass(x =>
        {
            while (x.Next(MoveType.After, [ILMatch.LdcR4(320f)]))
            {
                cursor.EmitDelegate((float x) => {
                    if (WiderSetModule.IsWide)
                    {
                        return x + 100;
                    }
                    return x;
                });
            }
        });

        cursor.Index = 0;

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.LdcI4(160)
            ]
        );

        cursor.EmitDelegate((int x) => {
            if (WiderSetModule.IsWide)
            {
                return x + 50;
            }
            return x;
        });

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.LdcR4(-320f)
            ]
        );

        cursor.EmitDelegate((float x) => {
            if (WiderSetModule.IsWide)
            {
                return x - 100;
            }
            return x;
        });


        return cursor.Generate();
    }

    private static IEnumerable<CodeInstruction> OrbLogic_CancelVariants_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(
            MoveType.After,
            [
                ILMatch.LdcR4(320)
            ]
        );

        cursor.EmitDelegate((float x) => {
            if (WiderSetModule.IsWide)
            {
                return x + 100;
            }
            return x;
        });

        return cursor.Generate();
    }
}