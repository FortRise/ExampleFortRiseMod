using System;
using System.Collections.Generic;
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
            transpiler: new HarmonyMethod(VersusRoundResults_ctor_Transpiler),
            finalizer: new HarmonyMethod(VersusRoundResults_ctor_Finalizer)
        );
    }

    private static IEnumerable<CodeInstruction> VersusRoundResults_ctor_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(
            [
                ILMatch.Ldstr("Invalid player amount for match results!")
            ]
        );

        // tweenFrom
        cursor.EmitDelegate<Func<Vector2[]>>(() =>
        {
            return TFGame.PlayerAmount switch
            {
                5 => [
                    new Vector2(-240f, 120f),
                    new Vector2(-160f, 120f),
                    new Vector2(210f, 360f),
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
        cursor.Emit(new CodeInstruction(OpCodes.Stloc_0));

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
                    new Vector2(50f, 105f),
                    new Vector2(130f, 105f),
                    new Vector2(210f, 105f),
                    new Vector2(110f, 210f),
                    new Vector2(190f, 210f),
                    new Vector2(170f, 210f)
                ],
                7 => [
                    new Vector2(40f, 105f),
                    new Vector2(120f, 105f),
                    new Vector2(200f, 105f),
                    new Vector2(280f, 105f),
                    new Vector2(80f, 210f),
                    new Vector2(160f, 210f),
                    new Vector2(240f, 210f)
                ],
                8 => [
                    new Vector2(20f, 105f),
                    new Vector2(100f, 105f),
                    new Vector2(180f, 105f),
                    new Vector2(260f, 105f),
                    new Vector2(60f, 210f),
                    new Vector2(140f, 210f),
                    new Vector2(220f, 210f),
                    new Vector2(300f, 210f)
                ],
                _ => []
            };
        });
        cursor.Emit(new CodeInstruction(OpCodes.Stloc_1));



        return cursor.Generate();
    }

    private static Exception? VersusRoundResults_ctor_Finalizer(Exception __exception)
    {
        return null;
    }
}