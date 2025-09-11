using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FortRise;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace Teuria.ScoreCounter;

internal sealed class QuestLevelSelectOverlayHooks : IHookable
{
    private static string totalScoreString = "0";

    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(QuestLevelSelectOverlay), "RefreshLevelStats"),
            postfix: new HarmonyMethod(QuestLevelSelectOverlay_RefreshLevelStats_Postfix)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(QuestLevelSelectOverlay), nameof(QuestLevelSelectOverlay.Render)),
            postfix: new HarmonyMethod(QuestLevelSelectOverlay_Render_Postfix)
        );
    }

    private static void QuestLevelSelectOverlay_Render_Postfix(QuestLevelSelectOverlay __instance)
    {
        if (totalScoreString != "0")
        {
            var drawStatsLerp = Private.Field<QuestLevelSelectOverlay, float>("drawStatsLerp", __instance);
            float num = MathHelper.Lerp(320f, 400f, Ease.CubeInOut(drawStatsLerp.Read()));
            if (TFGame.PlayerAmount == 1)
            {
                Draw.OutlineTextJustify(TFGame.Font, "SOLO SCORE:", new Vector2(num - 8, 62f), Color.LightGray, Color.Black, new Vector2(1f, 0.5f));
            }
            else
            {
                Draw.OutlineTextJustify(TFGame.Font, "TEAM SCORE:", new Vector2(num - 8, 62f), Color.LightGray, Color.Black, new Vector2(1f, 0.5f));
            }
            Draw.OutlineTextJustify(TFGame.Font, totalScoreString, new Vector2(num - 12f, 70f), Color.White, Color.Black, new Vector2(1f, 0.5f));
        }
    }

    private static void QuestLevelSelectOverlay_RefreshLevelStats_Postfix(QuestLevelSelectOverlay __instance)
    {
        var map = Private.Field<QuestLevelSelectOverlay, MapScene>("map", __instance).Read();
        if (map.Selection.Data is null)
        {
            totalScoreString = "0";
            return;
        }

        string id = map.Selection.Data.LevelData.LevelID;

        ref var towerStats = ref CollectionsMarshal.GetValueRefOrNullRef(ScoreCounterModule.SaveData.QuestTowers, id);

        if (Unsafe.IsNullRef(ref towerStats))
        {
            totalScoreString = "0";
            return;
        }

        if (TFGame.PlayerAmount == 1)
        {
            totalScoreString = towerStats.SoloScore.ToString();
        }
        else 
        {
            totalScoreString = towerStats.TeamScore.ToString();
        }
    }
}
