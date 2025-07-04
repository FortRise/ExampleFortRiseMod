using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using Monocle;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class VariantHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredConstructor(
                typeof(Variant),
                [
                    typeof(Subtexture),
                    typeof(string),
                    typeof(string),
                    typeof(Pickups[]),
                    typeof(bool),
                    typeof(string),
                    typeof(UnlockData.Unlocks?),
                    typeof(bool),
                    typeof(bool),
                    typeof(bool),
                    typeof(bool),
                    typeof(bool),
                    typeof(bool),
                    typeof(bool),
                    typeof(int)
                ]
            ),
            transpiler: new HarmonyMethod(Variant_ctor_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredPropertyGetter(typeof(Variant), nameof(Variant.Value)),
            transpiler: new HarmonyMethod(Variant_Value_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredPropertyGetter(typeof(Variant), nameof(Variant.AllTrue)),
            transpiler: new HarmonyMethod(Variant_AllTrue_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredPropertyGetter(typeof(Variant), nameof(Variant.Players)),
            transpiler: new HarmonyMethod(Variant_Players_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Variant), nameof(Variant.Clean)),
            transpiler: new HarmonyMethod(Variant_Clean_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> Variant_ctor_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(MoveType.After, [ILMatch.LdcI4(4)]);
        cursor.EmitDelegate((int x) => x + 4);

        return cursor.Generate();
    }

    private static IEnumerable<CodeInstruction> Variant_Value_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.Encompass(x =>
        {
            while (x.Next(MoveType.After, [ILMatch.LdcI4(4)]))
            {
                cursor.EmitDelegate((int x) => {
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

    private static IEnumerable<CodeInstruction> Variant_AllTrue_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(MoveType.After, [ILMatch.LdcI4(4)]);
        cursor.EmitDelegate((int x) => {
            if (WiderSetModule.IsWide)
            {
                return x + 4;
            }
            return x;
        });

        return cursor.Generate();
    }

    private static IEnumerable<CodeInstruction> Variant_Players_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(MoveType.After, [ILMatch.LdcI4(4)]);
        cursor.EmitDelegate((int x) => {
            if (WiderSetModule.IsWide)
            {
                return x + 4;
            }
            return x;
        });

        return cursor.Generate();
    }

    private static IEnumerable<CodeInstruction> Variant_Clean_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(MoveType.After, [ILMatch.LdcI4(4)]);
        cursor.EmitDelegate((int x) => {
            if (WiderSetModule.IsWide)
            {
                return x + 4;
            }
            return x;
        });

        return cursor.Generate();
    }
}