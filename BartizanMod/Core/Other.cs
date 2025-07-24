using System.Collections.Generic;
using FortRise;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using TowerFall;

namespace BartizanMod;

public class MyRollcallElement 
{
    public static void Register(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(RollcallElement), "ForceStart"),
            new HarmonyMethod(RollcallElement_ForceStart_Prefix)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(RollcallElement), "StartVersus"),
            new HarmonyMethod(RollcallElement_StartVersus_Prefix)
        );
    }

    private static void RollcallElement_StartVersus_Prefix()
    {
        var playerCount = WiderSetUtils.GetMenuPlayerCount();
        MyVersusPlayerMatchResults.PlayerWins = new int[playerCount];
    }

    private static void RollcallElement_ForceStart_Prefix()
    {
        var playerCount = WiderSetUtils.GetMenuPlayerCount();
        MyVersusPlayerMatchResults.PlayerWins = new int[playerCount];
    }
}

public class MyVersusPlayerMatchResults 
{
    public static int[] PlayerWins = null!;

    public static void Register(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredConstructor(typeof(VersusPlayerMatchResults),
            [
                typeof(Session),
                typeof(VersusMatchResults),
                typeof(int),
                typeof(Vector2),
                typeof(Vector2),
                typeof(List<AwardInfo>)
            ]),
            postfix: new HarmonyMethod(VersusPlayerMatchResults_ctor_Postfix)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(VersusPlayerMatchResults), nameof(VersusPlayerMatchResults.Render)),
            postfix: new HarmonyMethod(VersusPlayerMatchResults_Render_Postfix)
        );
    }

    private static void VersusPlayerMatchResults_ctor_Postfix(VersusPlayerMatchResults __instance, Session session, VersusMatchResults matchResults, int playerIndex)
    {
        var dynData = DynamicData.For(__instance);

        var gem = dynData.Get<Sprite<string>>("gem")!;
        if (session.MatchStats[playerIndex].Won)
        {
            PlayerWins[playerIndex]++;
        }

        if (PlayerWins[playerIndex] > 0) 
        {
            var winText = new OutlineText(TFGame.Font, PlayerWins[playerIndex].ToString(), gem.Position);
            winText.Color = Color.White;
            winText.OutlineColor = Color.Black;
            __instance.Add(winText);
            dynData.Set("winText", winText);
        }
    }

    private static void VersusPlayerMatchResults_Render_Postfix(VersusPlayerMatchResults __instance)
    {
        if (DynamicData.For(__instance).TryGet<OutlineText>("winText", out var text))
        {
            text.Render();
        }
    }
}