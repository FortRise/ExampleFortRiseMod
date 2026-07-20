using System;
using FortRise;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using TowerFall;

namespace Teuria.MoreReplay;

public sealed class TrialsControlPatch : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(TrialsControl), nameof(TrialsControl.Added)),
            postfix: new HarmonyMethod(TrialsControl_Added_Postfix)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(TrialsControl), nameof(TrialsControl.Update)),
            prefix: new HarmonyMethod(TrialsControl_Update_Prefix)
        );

        harmony.Patch(
            AccessTools.EnumeratorMoveNext(
                AccessTools.DeclaredMethod(typeof(TrialsControl), "WinSequence")),
            prefix: new HarmonyMethod(WinSequence_MoveNext_Prefix)
        );
    }

    private static void TrialsControl_Added_Postfix(TrialsControl __instance)
    {
        var onTweenOut = Private.Field<TrialsControl, Action<Tween>>("onTweenOut", __instance).Read();
        var entity = new Entity() 
        {
            Depth = -200
        };

        var text = new Text(TFGame.Font, "0.000", new Vector2(-160f, 5f), Text.HorizontalAlign.Left, Text.VerticalAlign.Top)
        {
            Scale = Vector2.One * 2f,
        };

        entity.Add(text);

        TrialsLevelData trialsLevelData = (__instance.Level.Session.MatchSettings.LevelSystem as TrialsLevelSystem)!.TrialsLevelData;
        TrialsLevelStats trialsLevelStats = SaveData.Instance.Trials.Levels[trialsLevelData.ID.X][trialsLevelData.ID.Y];

        Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 20, true);

        Text nextText;
        if (trialsLevelStats.NextGoal == -1)
        {
            nextText = new Text(
                TFGame.Font, 
                "BEST: " + TrialsResults.GetTimeString(trialsLevelStats.BestTime), 
                new Vector2(-160f, 22f), 
                Text.HorizontalAlign.Left, 
                Text.VerticalAlign.Center);

            tween.OnUpdate += (t) => 
            {
                nextText.X = MathHelper.Lerp(-160f, 5f, t.Eased);
            };

        }
        else 
        {
            nextText = new Text(
                TFGame.Font, 
                TrialsResults.GetTimeString(trialsLevelData.Goals[trialsLevelStats.NextGoal]), 
                new Vector2(-160f, 22f), 
                Text.HorizontalAlign.Left, 
                Text.VerticalAlign.Center);

            var nextImage = trialsLevelStats.GetNextSmallAwardIcon();
            nextImage.Play(0, false);
            nextImage.Position = new Vector2(-162f, 21f);
            entity.Add(nextImage);

            tween.OnUpdate += (t) => 
            {
                nextText.X = MathHelper.Lerp(-160f, 15f, t.Eased);
                nextImage.X = MathHelper.Lerp(-168f, 7f, t.Eased);
            };
        }

        entity.Add(nextText);


        __instance.Level.Add(entity);
        DynamicData.For(__instance).Set("Teuria.MoreReplay::replayTimer", text);

        tween.OnUpdate += (t) =>
        {
            text.X = MathHelper.Lerp(-160f, 5f, t.Eased);
        };
        __instance.Add(tween);
    }

    private static void TrialsControl_Update_Prefix(TrialsControl __instance)
    {
        var canEnd = Private.Field<TrialsControl, bool>("canEnd", __instance).Read();
        var time = Private.Field<TrialsControl, TimeSpan>("time", __instance).Read();
        var text = DynamicData.For(__instance).Get<Text>("Teuria.MoreReplay::replayTimer");
        if (text is null)
        {
            return;
        }

        if (canEnd)
        {
            text.DrawText = TrialsResults.GetTimeString(time);
        }
    }

    private static bool WinSequence_MoveNext_Prefix(object __instance, ref bool __result)
    {
        var traverseState = Traverse.Create(__instance).Field<int>("<>1__state");
        var state = traverseState.Value;

        if (state == 5)
        {
            __result = true;
            return false;
        }

        if (state == 3)
        {
            var instance = Traverse.Create(__instance).Field<TrialsControl>("<>4__this").Value;
            var entity = new Entity();
            Alarm.Set(entity, 10, () =>
            {
                entity.RemoveSelf();
                if (instance.Level.ReplayRecorder is null)
                {
                    AfterReplay();
                    return;
                }

                instance.Level.ReplayRecorder.End();
                instance.Level.ReplayViewer.Watch(
                    instance.Level.ReplayRecorder, ReplayViewer.ReplayType.Rewind, AfterReplay);

                void AfterReplay()
                {
                    traverseState.Value = 4;
                    instance.Level.Frozen = true;
                    DynamicData.For(instance).Invoke("TweenOut");
                    var text = DynamicData.For(instance).Get<Text>("Teuria.MoreReplay::replayTimer");
                    text!.Entity.RemoveSelf();
                }
            });
            instance.Level.Add(entity);

            traverseState.Value = 5;

            __result = true;
            return false;
        }
    
        return true;
    }
}

