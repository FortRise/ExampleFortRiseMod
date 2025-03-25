using System.Reflection;
using FortRise;
using Monocle;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using TowerFall;

namespace BartizanMod;

public class MyPlayer 
{
    internal static void Load() 
    {
        On.TowerFall.Player.CanGrabLedge += Player_CanGrabLedge_patch;
        On.TowerFall.Player.GetDodgeExitState += Player_GetDodgeExitState;
        On.TowerFall.Player.ShootArrow += Player_ShootArrow;
        On.TowerFall.Player.HurtBouncedOn += HurtBouncedOn_patch;
    }

    internal static void Unload() 
    {
        On.TowerFall.Player.CanGrabLedge -= Player_CanGrabLedge_patch;
        On.TowerFall.Player.GetDodgeExitState -= Player_GetDodgeExitState;
        On.TowerFall.Player.ShootArrow -= Player_ShootArrow;
        On.TowerFall.Player.HurtBouncedOn -= HurtBouncedOn_patch;
    }

    private static void HurtBouncedOn_patch(On.TowerFall.Player.orig_HurtBouncedOn orig, Player self, int bouncerIndex)
    {
        if (!VariantManager.GetCustomVariant("Bartizan/NoHeadBounce")[self.PlayerIndex])
            orig(self, bouncerIndex);
    }

    public delegate bool orig_Player_CanGrabLedge(Player self, int targetY, int direction);

    public static bool Player_CanGrabLedge_patch(On.TowerFall.Player.orig_CanGrabLedge orig, Player self, int targetY, int direction) 
    {
        if (VariantManager.GetCustomVariant("Bartizan/NoLedgeGrab")[self.PlayerIndex]) 
            return false;
        
        return orig(self, targetY, direction);
    }

    public static int Player_GetDodgeExitState(On.TowerFall.Player.orig_GetDodgeExitState orig, Player self) 
    {
        /* New */
        if (VariantManager.GetCustomVariant("Bartizan/NoDodgeCooldowns")[self.PlayerIndex]) 
        {
            var dynData = new DynData<Player>(self);
            dynData.Set("dodgeCooldown", false);
            dynData.Dispose();
        }
        return orig(self);
    }

    public static void Player_ShootArrow(On.TowerFall.Player.orig_ShootArrow orig, Player self) 
    {
        if (VariantManager.GetCustomVariant("Bartizan/InfiniteArrows")[self.PlayerIndex]) 
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
        if (VariantManager.GetCustomVariant("Bartizan/AwfullySlowArrows")[self.PlayerIndex]) 
        {
            Comp_TimeMult.SetValue(null, Engine.TimeMult * 0.2f, null);
            orig(self);
            Comp_TimeMult.SetValue(null, Engine.TimeMult / 0.2f, null);
            return;
        }
        if (VariantManager.GetCustomVariant("Bartizan/AwfullyFastArrows")[self.PlayerIndex]) 
        {
            Comp_TimeMult.SetValue(null, Engine.TimeMult * 3.0f, null);
            orig(self);
            Comp_TimeMult.SetValue(null, Engine.TimeMult / 3.0f, null);
            return;
        }
        orig(self);
    }
}