using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using Microsoft.Xna.Framework;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class VariantPerPlayerHooks : IHookable
{
    private static string[] playerTitles = ["P1", "P2", "P3", "P4", "P5", "P6", "P7", "P8"];
    private static Vector2[] playerPositions = [
        Vector2.UnitX * -80f,
        Vector2.UnitX * -55f,
        Vector2.UnitX * -30f,
        Vector2.UnitX * -5f,
        Vector2.UnitX * 20f,
        Vector2.UnitX * 45f,
        Vector2.UnitX * 70f,
        Vector2.UnitX * 95f,
    ];

    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredConstructor(typeof(VariantPerPlayer), [typeof(VariantToggle), typeof(Vector2)]),
            transpiler: new HarmonyMethod(VariantPerPlayer_ctor_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(VariantPerPlayer), nameof(VariantPerPlayer.Update)),
            transpiler: new HarmonyMethod(VariantPerPlayer_Update_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(VariantPerPlayer), nameof(VariantPerPlayer.Render)),
            transpiler: new HarmonyMethod(VariantPerPlayer_Render_Transpiler)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(VariantPerPlayer), "GetCursorTarget"),
            transpiler: new HarmonyMethod(VariantPerPlayer_GetCursorTarget_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> VariantPerPlayer_GetCursorTarget_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(MoveType.After, [ILMatch.Ldsfld("PlayerPositions")]);
        cursor.EmitDelegate((Vector2[] positions) =>
        {
            if (WiderSetModule.IsWide)
            {
                return playerPositions;
            }

            return positions;
        });

        return cursor.Generate();
    }

    private static IEnumerable<CodeInstruction> VariantPerPlayer_Render_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(MoveType.After, [ILMatch.LdcR4(80f)]);
        cursor.EmitDelegate((float width) =>
        {
            if (WiderSetModule.IsWide)
            {
                return width + 25;
            }

            return width;
        });

        cursor.GotoNext(MoveType.After, [ILMatch.LdcR4(160f)]);
        cursor.EmitDelegate((float width) =>
        {
            if (WiderSetModule.IsWide)
            {
                return width + 50;
            }

            return width;
        });

        cursor.GotoNext(MoveType.After, [ILMatch.Ldsfld("PlayerTitles")]);
        cursor.EmitDelegate((string[] positions) =>
        {
            if (WiderSetModule.IsWide)
            {
                return playerTitles;
            }

            return positions;
        });

        cursor.GotoNext(MoveType.After, [ILMatch.Ldsfld("PlayerPositions")]);
        cursor.EmitDelegate((Vector2[] positions) =>
        {
            if (WiderSetModule.IsWide)
            {
                return playerPositions;
            }

            return positions;
        });

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

    private static IEnumerable<CodeInstruction> VariantPerPlayer_Update_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.Encompass((x) =>
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

    private static IEnumerable<CodeInstruction> VariantPerPlayer_ctor_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.Encompass((x) =>
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