using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace Teuria.ScoreCounter;

internal sealed class QuestPlayerHUDHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredConstructor(typeof(QuestPlayerHUD), [typeof(QuestRoundLogic), typeof(Facing), typeof(int)]),
            transpiler: new HarmonyMethod(QuestPlayerHUD_ctor_Transpiler),
            postfix: new HarmonyMethod(QuestPlayerHUD_ctor_Postfix)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(QuestPlayerHUD), nameof(QuestPlayerHUD.Render)),
            postfix: new HarmonyMethod(QuestPlayerHUD_Render_Postfix)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Entity), nameof(Entity.Update)),
            postfix: new HarmonyMethod(Entity_Update_Postfix)
        );
    }

    private static IEnumerable<CodeInstruction> QuestPlayerHUD_ctor_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(MoveType.After, [ILMatch.LdcR4(13f)]);
        cursor.EmitDelegate((float y) => 
        {
            return y + 4;
        });

        return cursor.Generate();
    }

    private static void Entity_Update_Postfix(Entity __instance)
    {
        if (__instance.GetType() == typeof(QuestPlayerHUD))
        {
            var hud = __instance as QuestPlayerHUD;

            if (hud is null)
            {
                return;
            }

            var index = hud.PlayerIndex;

            var scoreData = ScoreCounterModule.Instance.ScoreData;
            if (scoreData.Scores[index] > scoreData.ScoreLast[index])
            {
                scoreData.ScoreLast[index] = Math.Min(scoreData.Scores[index], scoreData.ScoreLast[index] + 20);
                scoreData.ScoreText[index] = scoreData.ScoreLast[index].ToString();
                scoreData.ScoreScale[index] = 1.4f;
            }

            scoreData.ScoreScale[index] = Math.Max(1f, scoreData.ScoreScale[index] - 0.05f * Engine.TimeMult);
        }
    }

    private static void QuestPlayerHUD_ctor_Postfix(QuestPlayerHUD __instance)
    {
        ScoreCounterModule.Instance.ScoreData.ScoreText[__instance.PlayerIndex] = "0";
        ScoreCounterModule.Instance.ScoreData.ScoreLast[__instance.PlayerIndex] = 0;
        ScoreCounterModule.Instance.ScoreData.ScoreScale[__instance.PlayerIndex] = 1f;
    }

    private static void QuestPlayerHUD_Render_Postfix(QuestPlayerHUD __instance)
    {
        string scoreText = ScoreCounterModule.Instance.ScoreData.ScoreText[__instance.PlayerIndex];
        float scoreScale = ScoreCounterModule.Instance.ScoreData.ScoreScale[__instance.PlayerIndex];
        float x = TFGame.Font.MeasureString(scoreText).X;

        Color color = ArcherData.GetColorB(__instance.PlayerIndex);

        if (__instance.Side == Facing.Left)
        {
            Draw.OutlineTextCentered(TFGame.Font, scoreText, new Vector2(10f + x / 2f, 9f), color, scoreScale);
            return;
        }

        Draw.OutlineTextCentered(TFGame.Font, scoreText, new Vector2(310f - x / 2f, 9f), color, scoreScale);
    }
}
