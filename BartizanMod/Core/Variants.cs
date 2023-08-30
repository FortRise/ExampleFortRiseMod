using System.Reflection;
using Monocle;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using TowerFall;

namespace BartizanMod;

public class MyPlayer 
{
    private static IDetour hook_CanGrabLedge;
    private static IDetour hook_GetDodgeExitState;
    private static IDetour hook_ShootArrow;

    internal static void Load() 
    {
        hook_CanGrabLedge = new Hook(
            typeof(Player).GetMethod("CanGrabLedge", BindingFlags.NonPublic | BindingFlags.Instance),
            Player_CanGrabLedge_patch
        );
        hook_GetDodgeExitState = new Hook(
            typeof(Player).GetMethod("GetDodgeExitState", BindingFlags.NonPublic | BindingFlags.Instance),
            Player_GetDodgeExitState
        );
        hook_ShootArrow = new Hook(
            typeof(Player).GetMethod("ShootArrow", BindingFlags.NonPublic | BindingFlags.Instance),
            Player_ShootArrow
        );
        On.TowerFall.Player.HurtBouncedOn += HurtBouncedOn_patch;
    }

    internal static void Unload() 
    {
        hook_CanGrabLedge.Dispose();
        hook_GetDodgeExitState.Dispose();
        hook_ShootArrow.Dispose();
        On.TowerFall.Player.HurtBouncedOn -= HurtBouncedOn_patch;
    }

    private static void HurtBouncedOn_patch(On.TowerFall.Player.orig_HurtBouncedOn orig, Player self, int bouncerIndex)
    {
        var matchVariants = self.Level.Session.MatchSettings.Variants;
        if (!matchVariants.GetCustomVariant("NoHeadBounce")[self.PlayerIndex])
            orig(self, bouncerIndex);
    }

    public delegate bool orig_Player_CanGrabLedge(Player self, int targetY, int direction);

    public static bool Player_CanGrabLedge_patch(orig_Player_CanGrabLedge orig, Player self, int targetY, int direction) 
    {
        var matchVariants = self.Level.Session.MatchSettings.Variants;
        if (matchVariants.GetCustomVariant("NoLedgeGrab")[self.PlayerIndex]) 
            return false;
        
        return orig(self, targetY, direction);
    }

    public delegate int orig_Player_GetDodgeExitState(Player self);

    public static int Player_GetDodgeExitState(orig_Player_GetDodgeExitState orig, Player self) 
    {
        var matchVariants = self.Level.Session.MatchSettings.Variants;
        if (matchVariants.GetCustomVariant("NoDodgeCooldowns")[self.PlayerIndex]) 
        {
            var dynData = new DynData<Player>(self);
            dynData.Set("dodgeCooldown", false);
            dynData.Dispose();
        }
        return orig(self);
    }

    public delegate void orig_Player_ShootArrow(Player self);

    public static void Player_ShootArrow(orig_Player_ShootArrow orig, Player self) 
    {
        var matchVariants = self.Level.Session.MatchSettings.Variants;
        if (matchVariants.GetCustomVariant("InfiniteArrows")[self.PlayerIndex]) 
        {
            var arrow = self.Arrows.Arrows[0];
            orig(self);
            self.Arrows.AddArrows(arrow);
            return;
        }
        orig(self);
    }
}

public class MyArrow 
{
    private static PropertyInfo Comp_TimeMult;

    internal static void Load() 
    {
        On.TowerFall.Arrow.ArrowUpdate += ArrowUpdate_patch;
        Comp_TimeMult = typeof(Engine).GetProperty("TimeMult");
    }

    internal static void Unload() 
    {
        On.TowerFall.Arrow.ArrowUpdate -= ArrowUpdate_patch;
    }


    private static void ArrowUpdate_patch(On.TowerFall.Arrow.orig_ArrowUpdate orig, Arrow self)
    {
        var matchVariants = self.Level.Session.MatchSettings.Variants;

        if (matchVariants.GetCustomVariant("AwfullySlowArrows")) 
        {
            Comp_TimeMult.SetValue(null, Engine.TimeMult * 0.2f, null);
            orig(self);
            Comp_TimeMult.SetValue(null, Engine.TimeMult / 0.2f, null);
            return;
        }
        if (matchVariants.GetCustomVariant("AwfullyFastArrows")) 
        {
            Comp_TimeMult.SetValue(null, Engine.TimeMult * 3.0f, null);
            orig(self);
            Comp_TimeMult.SetValue(null, Engine.TimeMult / 3.0f, null);
            return;
        }
        orig(self);
    }
}