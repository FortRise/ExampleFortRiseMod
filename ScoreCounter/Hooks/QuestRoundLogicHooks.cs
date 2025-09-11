using System;
using FortRise;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace Teuria.ScoreCounter;

internal sealed class QuestRoundLogicHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredConstructor(typeof(QuestRoundLogic), [typeof(Session)]),
            prefix: new HarmonyMethod(QuestRoundLogic_ctor_Prefix)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(QuestRoundLogic), nameof(QuestRoundLogic.RegisterEnemyKill)),
            prefix: new HarmonyMethod(QuestRoundLogic_RegisterEnemyKill_Prefix)
        );
    }

    private static void QuestRoundLogic_ctor_Prefix()
    {
        Array.Fill(ScoreCounterModule.Instance.ScoreData.Scores, 0);
    }

    private static void QuestRoundLogic_RegisterEnemyKill_Prefix(
        QuestRoundLogic __instance,
        Vector2 at, 
        int killerIndex, 
        int points)
    {
        if (points <= 0)
        {
            return;
        }

        if (killerIndex != -1 && TFGame.Players[killerIndex])
        {
            int[] combos = Private.Field<QuestRoundLogic, int[]>("combos", __instance).Read();
            int combo = combos[killerIndex] + 1;
            int score = points + (Math.Min(combo, 6) - 1) * 100;

            var popup = Cache.Create<QuestScorePopup>();
            popup.Initialize(at, score, killerIndex);
            __instance.Session.CurrentLevel.Add(popup);
            
            ScoreCounterModule.Instance.ScoreData.Scores[killerIndex] += score;
        }
    }
}
