using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using TowerFall;

namespace BartizanMod;

public class MyRollcallElement 
{
    internal static void Load() 
    {
        On.TowerFall.RollcallElement.ForceStart += ForceStart_patch;
        On.TowerFall.RollcallElement.StartVersus += StartVersus_patch;
    }

    internal static void Unload() 
    {
        On.TowerFall.RollcallElement.ForceStart -= ForceStart_patch;
        On.TowerFall.RollcallElement.StartVersus -= StartVersus_patch;
    }

    private static void StartVersus_patch(On.TowerFall.RollcallElement.orig_StartVersus orig, RollcallElement self)
    {
        var playerCount = EightPlayerUtils.GetMenuPlayerCount();
        MyVersusPlayerMatchResults.PlayerWins = new int[playerCount];
        orig(self);
    }

    private static void ForceStart_patch(On.TowerFall.RollcallElement.orig_ForceStart orig, RollcallElement self)
    {
        var playerCount = EightPlayerUtils.GetMenuPlayerCount();
        MyVersusPlayerMatchResults.PlayerWins = new int[playerCount];
        orig(self);
    }
}

public class MyVersusPlayerMatchResults 
{
    public static int[] PlayerWins = null!;


    internal static void Load() 
    {
        On.TowerFall.VersusPlayerMatchResults.ctor += ctor_patch;
        On.TowerFall.VersusPlayerMatchResults.Render += Render_patch;
    }

    internal static void Unload() 
    {
        On.TowerFall.VersusPlayerMatchResults.ctor -= ctor_patch;
        On.TowerFall.VersusPlayerMatchResults.Render -= Render_patch;
    }

    private static void ctor_patch(On.TowerFall.VersusPlayerMatchResults.orig_ctor orig, VersusPlayerMatchResults self, Session session, VersusMatchResults matchResults, int playerIndex, Vector2 tweenFrom, Vector2 tweenTo, List<AwardInfo> awards)
    {
        orig(self, session, matchResults, playerIndex, tweenFrom, tweenTo, awards);
        var dynData = DynamicData.For(self);
        var gem = dynData.Get<Sprite<string>>("gem")!;
        if (session.MatchStats[playerIndex].Won)
            PlayerWins[playerIndex]++;

        if (PlayerWins[playerIndex] > 0) 
        {
            var winText = new OutlineText(TFGame.Font, PlayerWins[playerIndex].ToString(), gem.Position);
            winText.Color = Color.White;
            winText.OutlineColor = Color.Black;
            self.Add(winText);
            dynData.Set("winText", winText);
        }
    }

    private static void Render_patch(On.TowerFall.VersusPlayerMatchResults.orig_Render orig, VersusPlayerMatchResults self)
    {
        orig(self);
        if (DynamicData.For(self).TryGet<OutlineText>("winText", out var text)) 
            text.Render();
    }
}