using System.Reflection;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using TowerFall;

namespace BartizanMod;

public class MyRollcallElement 
{
    private static IDetour hook_ForceStart;
    private static IDetour hook_StartVersus;

    internal static void Load() 
    {
        hook_ForceStart = new Hook(
            typeof(RollcallElement).GetMethod("ForceStart", BindingFlags.NonPublic | BindingFlags.Instance),
            ForceStart_patch
        );
        hook_StartVersus = new Hook(
            typeof(RollcallElement).GetMethod("StartVersus", BindingFlags.NonPublic | BindingFlags.Instance),
            StartVersus_patch
        );
    }

    internal static void Unload() 
    {
        hook_ForceStart.Dispose();
        hook_StartVersus.Dispose();
    }

    public delegate void orig_RollcallElement_ForceStart(RollcallElement self);

    public static void ForceStart_patch(orig_RollcallElement_ForceStart orig, RollcallElement self) 
    {
        MyVersusPlayerMatchResults.PlayerWins = new int[4];
        orig(self);
    }


    public delegate void orig_RollcallElement_StartVersus(RollcallElement self);

    public static void StartVersus_patch(orig_RollcallElement_StartVersus orig, RollcallElement self) 
    {
        MyVersusPlayerMatchResults.PlayerWins = new int[4];
        orig(self);
    }
}

public class MyVersusPlayerMatchResults 
{
    public static int[] PlayerWins;

    private static OutlineText[] winsTexts = new OutlineText[4];

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
        var gem = dynData.Get<Sprite<string>>("gem");
        if (session.MatchStats[playerIndex].Won)
            PlayerWins[playerIndex]++;

        if (PlayerWins[playerIndex] > 0) {
            winsTexts[playerIndex] = new OutlineText(TFGame.Font, PlayerWins[playerIndex].ToString(), gem.Position);
            winsTexts[playerIndex].Color = Color.White;
            winsTexts[playerIndex].OutlineColor = Color.Black;
            self.Add(winsTexts[playerIndex]);
        }
    }

    private static void Render_patch(On.TowerFall.VersusPlayerMatchResults.orig_Render orig, VersusPlayerMatchResults self)
    {
        orig(self);
        var playerIndex = DynamicData.For(self).Get<int>("playerIndex");
        if (winsTexts[playerIndex] != null)
            winsTexts[playerIndex].Render();
    }
}