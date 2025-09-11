using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace Teuria.ScoreCounter;

internal sealed class QuestCompleteHooks : IHookable
{
    private static QuestComplete instance = null!;
    private static Wiggler totalWiggler = null!;
    private static OutlineText totalScore = null!;

    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(QuestComplete), "Sequence"),
            postfix: new HarmonyMethod(QuestComplete_Sequence_Postfix)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(QuestComplete), "ShowPlayerStats"),
            postfix: new HarmonyMethod(QuestComplete_ShowPlayerStats_Postfix)
        );

        harmony.Patch(
            AccessTools.EnumeratorMoveNext(
                AccessTools.DeclaredMethod(typeof(QuestComplete), "Sequence")
            ),
            transpiler: new HarmonyMethod(QuestComplete_Sequence_Transpiler)
        );
    }

    private static IEnumerable<CodeInstruction> QuestComplete_Sequence_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(MoveType.After, [ILMatch.Callvirt("TweenNextGemTo")]);
        cursor.EmitDelegate(() => 
        {
            ScoreCounterModule.Instance.ScoreData.ScoreBonuses += 1000;
            totalScore.DrawText = ScoreCounterModule.Instance.ScoreData.TotalScore.ToString();
            totalWiggler.Start();
        });

        cursor.GotoNext(MoveType.After, [ILMatch.Call("InvokeQuestComplete_Result")]);
        cursor.EmitDelegate(() => 
        {
            var roundLogic = Private.Field<QuestComplete, QuestRoundLogic>("quest", instance).Read();
            int bonus = (int)Calc.Snap((float)(600.0 - TimeSpan.FromTicks(roundLogic.Time).TotalSeconds) * 10f, 10f);
            ScoreCounterModule.Instance.ScoreData.ScoreBonuses += bonus;

            if (roundLogic.Session.MatchSettings.LevelSystem is QuestLevelSystem levelSystem)
            {
                var towerData = levelSystem.QuestTowerData;
                ref var towerStats = ref CollectionsMarshal.GetValueRefOrAddDefault(ScoreCounterModule.SaveData.QuestTowers, towerData.LevelID, out bool exists)!;

                if (!exists)
                {
                    towerStats = new ScoreCounterSaveData.ScoreQuestTower();
                }

                if (TFGame.PlayerAmount == 1)
                {
                    towerStats.SoloScore = 
                        Math.Max(towerStats.SoloScore, ScoreCounterModule.Instance.ScoreData.TotalScore);
                }
                else 
                {
                    towerStats.TeamScore = 
                        Math.Max(towerStats.TeamScore, ScoreCounterModule.Instance.ScoreData.TotalScore);
                }
            }
        });

        cursor.GotoNext([ILMatch.Ldsfld("sfx_complete6timeTally")]);
        cursor.EmitDelegate(() => 
        {
            Alarm.Set(instance, 60, () => 
            {
                totalScore.DrawText = ScoreCounterModule.Instance.ScoreData.TotalScore.ToString();
                totalScore.Color = Calc.HexToColor("FFEE35");
                totalScore.Scale = Vector2.One * 1.5f;
                totalWiggler.RemoveSelf();

                totalWiggler = Wiggler.Create(20, 4f, onChange: v => 
                {
                    totalScore.Scale = Vector2.One * (1.5f + v * 0.3f);
                });

                instance.Add(totalWiggler);

                totalWiggler.Start();
                Sounds.sfx_complete7totalScore.Play(160f, 1f);
            }, Alarm.AlarmMode.Oneshot);
        });

        return cursor.Generate();
    }

    private static void QuestComplete_Sequence_Postfix(QuestComplete __instance, ref IEnumerator __result)
    {
        instance = __instance;
    }

    private static void QuestComplete_ShowPlayerStats_Postfix(QuestComplete __instance, ref IEnumerator __result)
    {
        __result = EnumeratorHandlerShowPlayerStats(__result);
    }
    
    private static IEnumerator EnumeratorHandlerShowPlayerStats(IEnumerator __result)
    {
        yield return __result;
        CreateScoreText();
    }

    private static void CreateScoreText()
    {
        totalScore = new OutlineText(
            TFGame.Font, 
            ScoreCounterModule.Instance.ScoreData.TotalScore.ToString(),
            new Vector2(160f, 135f)
        );
        
        totalScore.Color = Color.Transparent;
        totalScore.OutlineColor = Color.Transparent;
        totalScore.Scale = Vector2.One;

        instance.Add(totalScore);

        Vector2 start = totalScore.Position + Vector2.UnitY * 20f;
        Vector2 end = totalScore.Position; 

        Color textColor = Calc.HexToColor("FFFFFF");

        var tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 60, true);
        tween.OnUpdate = t => 
        {
            totalScore.Position = Vector2.Lerp(start, end, t.Eased);
            totalScore.OutlineColor = Color.Black * t.Eased * t.Eased;
            totalScore.Color = textColor * t.Eased;
        };

        instance.Add(tween);

        totalWiggler = Wiggler.Create(20, 4f, onChange: v => 
        {
            totalScore.Scale = Vector2.One * (1f + v * 0.3f);
        });

        instance.Add(totalWiggler);

        Sounds.sfx_complete6timeTally.Play(160f, 1f);
    }
}
