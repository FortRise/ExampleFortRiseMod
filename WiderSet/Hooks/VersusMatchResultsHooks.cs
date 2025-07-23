using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class VersusMatchResultsHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredConstructor(typeof(VersusMatchResults), [typeof(Session), typeof(VersusRoundResults)]),
            transpiler: new HarmonyMethod(VersusMatchResults_ctor_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> VersusMatchResults_ctor_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(MoveType.After, [ILMatch.Stloc(6), ILMatch.Ldloc(6)]);
        cursor.GotoNext(MoveType.After, [ILMatch.Stloc(6), ILMatch.Ldloc(6)]);
        cursor.GotoNext(MoveType.After, [ILMatch.Stloc(6), ILMatch.Ldloc(6)]);


        var br_Winner = cursor.CreateLabel();

        cursor.GotoNext(
            [
                ILMatch.Ldstr("Invalid player amount for match results!")
            ]
        );

        var instr = cursor.Instructions[cursor.Index];
        var labels = instr.labels.ToHashSet();
        cursor.Instructions[cursor.Index] = new CodeInstruction(instr.opcode, instr.operand)
        {
            blocks = [.. instr.blocks]
        };

        // tweenFrom
        cursor.EmitDelegate<Func<Vector2[]>>(() =>
        {
            return TFGame.PlayerAmount switch
            {
                5 => [
                    new Vector2(-240f, 120f),
                    new Vector2(-160f, 120f),
                    new Vector2(160f, 360f),
                    new Vector2(580f, 120f),
                    new Vector2(660f, 120f)
                ],
                6 => [
                    new Vector2(-660f, 120f),
                    new Vector2(-660f, 120f),
                    new Vector2(-660f, 360f),
                    new Vector2(660f, 360f),
                    new Vector2(660f, 120f),
                    new Vector2(660f, 120f),
                ],
                7 => [
                    new Vector2(-660f, 0f),
                    new Vector2(-660f, 0f),
                    new Vector2(-660f, 0f),
                    new Vector2(660f, 0f),
                    new Vector2(660f, 0f),
                    new Vector2(660f, 0f),
                    new Vector2(660f, 0f),
                ],
                8 => [
                    new Vector2(-660f, 0f),
                    new Vector2(-660f, 0f),
                    new Vector2(-660f, 0f),
                    new Vector2(660f, 0f),
                    new Vector2(660f, 0f),
                    new Vector2(660f, 0f),
                    new Vector2(660f, 0f),
                    new Vector2(660f, 0f),
                ],
                _ => []
            };
        });

        foreach (var label in labels)
        {
            cursor.Prev.labels.Add(label);
        }

        cursor.Emit(new CodeInstruction(OpCodes.Stloc_0));

        const float yTop = 75f;
        const float yBottom = 180f;

        // tweenTo
        cursor.EmitDelegate<Func<Vector2[]>>(() =>
        {
            var offset = Screen.LeftImage.Width;
            return TFGame.PlayerAmount switch
            {
                5 => [
                    new Vector2(0f, 120f),
                    new Vector2(80f, 120f),
                    new Vector2(160f, 120f),
                    new Vector2(240f, 120f),
                    new Vector2(320f, 120f)
                ],
                6 => [
                    new Vector2(50f, yTop),
                    new Vector2(130f, yTop),
                    new Vector2(210f, yTop),
                    new Vector2(110f, yBottom),
                    new Vector2(190f, yBottom),
                    new Vector2(270f, yBottom)
                ],
                7 => [
                    new Vector2(40f, yTop),
                    new Vector2(120f, yTop),
                    new Vector2(200f, yTop),
                    new Vector2(280f, yTop),
                    new Vector2(80f, yBottom),
                    new Vector2(160f, yBottom),
                    new Vector2(240f, yBottom)
                ],
                8 => [
                    new Vector2(20f, yTop),
                    new Vector2(100f, yTop),
                    new Vector2(180f, yTop),
                    new Vector2(260f, yTop),
                    new Vector2(60f, yBottom),
                    new Vector2(140f, yBottom),
                    new Vector2(220f, yBottom),
                    new Vector2(300f, yBottom)
                ],
                _ => []
            };
        });
        cursor.Emit(new CodeInstruction(OpCodes.Stloc_1));

        cursor.Emit(OpCodes.Br_S, br_Winner);

        cursor.GotoNext([ILMatch.Throw()]);

        cursor.MarkLabel(br_Winner);

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
