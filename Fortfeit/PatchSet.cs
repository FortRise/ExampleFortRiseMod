using FortRise;
using HarmonyLib;
using Microsoft.Xna.Framework.Input;
using Monocle;
using MonoMod.Utils;
using TowerFall;

namespace Teuria.Fortfeit;

[HarmonyPatch]
public static class PatchSet
{
    [HarmonyPatch(typeof(VersusRoundResults), "SaveReplay")]
    [HarmonyPostfix]
    public static void VersusRoundResuls_SaveReplay_Postfix()
    {
        if (TempLifetime.RoundReults.TryGetTarget(out var guide))
        {
            guide?.Visible = false;
        }
    }

    [HarmonyPatch(typeof(VersusRoundResults), "FinishSaveReplay")]
    [HarmonyPostfix]
    public static void VersusRoundResuls_FinishSaveReplay_Postfix(VersusRoundResults __instance)
    {
        var forfeit = DynamicData.For(__instance).Get("Teuria.Forfeit::forfeit");

        if (forfeit is not bool isForfeit)
        {
            return;
        }

        if (isForfeit)
        {
            return;
        }

        if (TempLifetime.RoundReults.TryGetTarget(out var guide))
        {
            guide?.Visible = true;
        }
    }

    [HarmonyPatch(typeof(VersusRoundResults), nameof(VersusRoundResults.TweenOut))]
    [HarmonyPostfix]
    public static void VersusRoundResuls_TweenOut_Postfix()
    {
        if (TempLifetime.RoundReults.TryGetTarget(out var guide))
        {
            guide?.Visible = false;
        }
    }

    [HarmonyPatch(typeof(VersusRoundResults), nameof(VersusRoundResults.Update))]
    [HarmonyPostfix]
    public static void VersusRoundResuls_Update_Postfix(VersusRoundResults __instance)
    {
        var finished = Private.Field<VersusRoundResults, bool>("finished", __instance).Read();

        if (!finished)
        {
            return;
        }

        if (__instance.MatchResults != null)
        {
            return;
        }

        bool pressed = false;

        PlayerInput? playerInput = null;
        for (int i = 0; i < 4; i++)
        {
            if (TFGame.PlayerInputs[i] != null)
            {
                playerInput = TFGame.PlayerInputs[i];
                break;
            }
        }

        if (playerInput is not null)
        {
            if (playerInput is KeyboardInput keyInput)
            {
                pressed = MInput.Keyboard.Pressed(keyInput.Config.Arrows[0]);
            }
            else if (playerInput is XGamepadInput xGamepadInput)
            {
                pressed = xGamepadInput.XGamepad.Pressed(Buttons.Y);
            }
        }

        if (!pressed)
        {
            return;
        }

        var session = __instance.Level.Session;
        DynamicData.For(session).Set("Teuria.Forfeit::forfeit", true);
        __instance.MatchResults = new VersusMatchResults(session, __instance);
        session.CurrentLevel.Add(__instance.MatchResults);
        __instance.TweenOut();
        __instance.MatchResults.TweenIn();

        int winner = session.GetWinner();

        if (session.MatchSettings.TeamMode)
        {
            ArcherData.Teams[winner].PlayVictoryMusic();
        }
        else
        {
            ArcherData.Get(TFGame.Characters[winner], TFGame.AltSelect[winner]).PlayVictoryMusic();
        }
    }

    [HarmonyPatch(typeof(Session), nameof(Session.GetWinner))]
    [HarmonyPostfix]
    public static void Session_GetWinner_Postfix(Session __instance, ref int __result)
    {
        if (__result != -1)
        {
            return;
        }

        var forfeit = DynamicData.For(__instance).Get("Teuria.Forfeit::forfeit");

        if (forfeit is not bool isForfeit)
        {
            return;
        }

        if (!isForfeit)
        {
            return;
        }

        int currentIndexLead = 0;
        int highScore = 0;

        for (int i = 0; i < __instance.Scores.Length; i += 1)
        {
            if (__instance.Scores[i] > highScore)
            {
                highScore = __instance.Scores[i];
                currentIndexLead = i;
            }
        }

        __result = currentIndexLead;
    }
}
