using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using FortRise;
using FortRise.Transpiler;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace Teuria.WiderSet;

internal sealed class VersusPlayerMatchResultsHooks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredConstructor(typeof(VersusPlayerMatchResults), [typeof(Session), typeof(VersusMatchResults), typeof(int), typeof(Vector2), typeof(Vector2), typeof(List<AwardInfo>)]),
            transpiler: new HarmonyMethod(VersusPlayerMatchResults_ctor_Transpiler),
            postfix: new HarmonyMethod(VersusPlayerMatchResults_ctor_Postfix)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(VersusPlayerMatchResults), nameof(VersusPlayerMatchResults.DoSequence)),
            new HarmonyMethod(VersusPlayerMatchResults_DoSequence_Prefix)
        );
    }

    private static void VersusPlayerMatchResults_ctor_Postfix(VersusPlayerMatchResults __instance)
    {
        if (TFGame.PlayerAmount > 5)
        {
            var rect = __instance.GetFirst<DrawRectangle>();
            __instance.Remove(rect);

            var portrait = Private.Field<VersusPlayerMatchResults, Image>("portrait", __instance).Read();
            var gem = Private.Field<VersusPlayerMatchResults, Sprite<string>>("gem", __instance).Read();

            portrait.Scale = Vector2.One * 0.7f;
            portrait.Position.Y -= 10f;
            gem.Scale = Vector2.One * 0.7f;
            gem.Position.Y += 5f;
        }
    }

    private static bool VersusPlayerMatchResults_DoSequence_Prefix(VersusPlayerMatchResults __instance)
    {
        if (TFGame.PlayerAmount > 5)
        {
            __instance.Add(new Coroutine(SmallSequence(__instance)));
            return false;
        }
        return true;
    }

    private static IEnumerator SmallSequence(VersusPlayerMatchResults __instance) 
    {
        var portrait = Private.Field<VersusPlayerMatchResults, Image>("portrait", __instance).Read();
        var session = Private.Field<VersusPlayerMatchResults, Session>("session", __instance).Read();
        var playerIndex = Private.Field<VersusPlayerMatchResults, int>("playerIndex", __instance).Read();
        var awards = Private.Field<VersusPlayerMatchResults, List<AwardInfo>>("awards", __instance).Read();

        yield return 30;
        Vector2 end2 = portrait.Position + Vector2.UnitY * (portrait.Height / 2f + 2f);
        Vector2 start2 = end2 + Vector2.UnitX * -20f;
        ulong kills = session.MatchStats[playerIndex].Kills.Kills;
        ulong deaths = session.MatchStats[playerIndex].Deaths.Total - session.MatchStats[playerIndex].Deaths.Environment;
        ulong consequence;
        string textConsequence;

        if (__instance.Level.Session.MatchSettings.TeamMode && __instance.Level.Session.MatchSettings.Variants!.TeamRevive)
        {
            consequence = session.MatchStats[playerIndex].Revives;
            textConsequence = "R";
        }
        else if (__instance.Level.Session.MatchSettings.TeamMode)
        {
            consequence = session.MatchStats[playerIndex].Kills.SelfKills + session.MatchStats[playerIndex].Kills.TeamKills + session.MatchStats[playerIndex].Deaths.Environment;
            textConsequence = "T";
        }
        else
        {
            consequence = session.MatchStats[playerIndex].Deaths.SelfKills + session.MatchStats[playerIndex].Deaths.Environment;
            textConsequence = "S";
        }

        var killDeathSelf = new OutlineText(TFGame.Font, $"K: {kills}/D: {deaths}/{textConsequence}: {consequence}");
        killDeathSelf.Color = Color.Transparent;
        killDeathSelf.OutlineColor = Color.Transparent;
        __instance.Add(killDeathSelf);
        Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 30, true);
        tween.OnUpdate = delegate(Tween t)
        {
            killDeathSelf.Position = Vector2.Lerp(start2, end2, t.Eased);
            killDeathSelf.Color = Color.White * t.Eased;
            killDeathSelf.OutlineColor = Color.Black * t.Eased * t.Eased;
        };
        __instance.Add(tween);

        yield return 30;
        int num5;
        for (int i = 0; i < awards.Count; i = num5 + 1)
        {
            AwardInfo awardInfo = awards[i];
            Vector2 iconEnd = portrait.Position + Vector2.UnitY * (portrait.Height / 2f + 16f + i * 15);
            Vector2 iconStart = iconEnd + Vector2.UnitX * -20f;
            Sprite<int> icon = awardInfo.GetSprite(false);
            icon.CenterOrigin();
            icon.Position = iconStart;
            icon.Scale = Vector2.One * 1.5f;
            icon.Color = Color.Transparent;
            icon.Play(0, false);
            int anim = 0;
            icon.OnAnimationComplete = delegate(Sprite<int> s)
            {
                if (!icon.Looping)
                {
                    Sprite<int> icon2 = icon;
                    int num6 = anim + 1;
                    anim = num6;
                    icon2.Play(num6, false);
                }
            };
            __instance.Add(icon);
            tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 20, true);
            tween.OnUpdate = delegate(Tween t)
            {
                icon.Color = Color.White * t.Eased;
                icon.Rotation = MathHelper.Lerp(-0.34906584f, 0f, t.Eased);
                icon.Position = Vector2.Lerp(iconStart, iconEnd, t.Eased);
            };
            tween.OnComplete = delegate(Tween t)
            {
                Tween tween2 = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeIn, 20, true);
                tween2.OnUpdate = delegate(Tween tt)
                {
                    icon.Scale = Vector2.One * MathHelper.Lerp(1.5f, 1f, tt.Eased);
                };
                __instance.Add(tween2);
            };
            __instance.Add(tween);
            if (i == 0)
            {
                Sounds.sfx_statscreenReward1.Play(__instance.X, 1f);
            }
            else if (i == 1)
            {
                Sounds.sfx_statscreenReward2.Play(__instance.X, 1f);
            }
            else
            {
                Sounds.sfx_statscreenReward3.Play(__instance.X, 1f);
            }
            yield return 40;
            num5 = i;
        }
        __instance.Finished = true;
    }

    private static IEnumerable<CodeInstruction> VersusPlayerMatchResults_ctor_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new ILTranspilerCursor(generator, instructions);

        cursor.GotoNext(MoveType.After, [ILMatch.Callvirt("get_Item")]);
        cursor.Emit(new CodeInstruction(OpCodes.Ldarg_0));
        cursor.EmitDelegate((Subtexture normalPortrait, VersusPlayerMatchResults __instance) =>
        {
            if (TFGame.PlayerAmount > 5)
            {
                var won = Private.Field<VersusPlayerMatchResults, bool>("won", __instance);
                if (won.Read())
                {
                    return WiderSetModule.GoldPortrait.Subtexture;
                }

                return WiderSetModule.SilverPortrait.Subtexture;
            }

            return normalPortrait;
        });

        return cursor.Generate();
    }
}