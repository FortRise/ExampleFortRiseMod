using System.Globalization;
using System.Reflection;
using Monocle;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using TowerFall;

namespace BartizanMod;

public class MyMatchVariants : MatchVariants
{
    internal static void Load() 
    {
        On.TowerFall.MatchVariants.ctor += ctor_patch;
    }

    internal static void Unload() 
    {
        On.TowerFall.MatchVariants.ctor -= ctor_patch;
    }

    private static void ctor_patch(On.TowerFall.MatchVariants.orig_ctor orig, MatchVariants self, bool noPerPlayer)
    {
        orig(self, noPerPlayer);
        var variantDescriptor = new VariantDescriptor(BartizanModModule.BartizanAtlas);
        var noHeadBounce = self.AddVariant(
            "NoHeadBounce", variantDescriptor with { Header = "MODS" }, VariantFlags.PerPlayer, noPerPlayer);
        var noDodgeCooldown = self.AddVariant(
            "NoDodgeCooldowns", variantDescriptor, VariantFlags.PerPlayer, noPerPlayer);
        var awfullyFastArrows = self.AddVariant(
            "AwfullyFastArrows", variantDescriptor, VariantFlags.None, noPerPlayer);
        var awfullySlowArrows = self.AddVariant(
            "AwfullySlowArrows", variantDescriptor, VariantFlags.None, noPerPlayer);
        var noLedgeGrab = self.AddVariant(
            "NoLedgeGrab", variantDescriptor, VariantFlags.PerPlayer, noPerPlayer);
        var infiniteArrows = self.AddVariant(
            "InfiniteArrows", variantDescriptor, VariantFlags.PerPlayer, noPerPlayer);

        self.CreateCustomLinks(noHeadBounce, self.NoTimeLimit);
        self.CreateCustomLinks(noDodgeCooldown, self.ShowDodgeCooldown);
        self.CreateCustomLinks(awfullyFastArrows, awfullySlowArrows);
    }
}

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
    internal static void Load() 
    {
        On.TowerFall.Arrow.ctor += Arrow_ctor;
        On.TowerFall.Arrow.ArrowUpdate += ArrowUpdate_patch;
    }

    internal static void Unload() 
    {
        On.TowerFall.Arrow.ctor -= Arrow_ctor;
        On.TowerFall.Arrow.ArrowUpdate -= ArrowUpdate_patch;
    }

    private static void Arrow_ctor(On.TowerFall.Arrow.orig_ctor orig, Arrow self)
    {
        orig(self);
        // Session is null, maybe we could have Added function ready in MMHook soon
        // var matchVariants = self.Level.Session.MatchSettings.Variants;
        // var awfullyFastArrows = matchVariants.GetCustomVariant("AwfullyFastArrows");
        // if (awfullyFastArrows == null)
        // {
        //     Engine.Instance.Commands.Log("AwfullyFastArrows returns null");
        //     return;
        // }

        // if (awfullyFastArrows) 
        // {
        //     var dynData = new DynData<Arrow>(self);
        //     dynData.Set<WrapHitbox>("NormalHitbox", new WrapHitbox(6f, 3f, -1f, -1f));
        //     dynData.Set<WrapHitbox>("otherArrowHitbox", new WrapHitbox(12f, 4f, -2f, -2f));
        //     dynData.Dispose();
        // }
    }

    private static void ArrowUpdate_patch(On.TowerFall.Arrow.orig_ArrowUpdate orig, Arrow self)
    {
        var matchVariants = self.Level.Session.MatchSettings.Variants;

        if (matchVariants.GetCustomVariant("AwfullySlowArrows")) 
        {
            typeof(Engine).GetProperty("TimeMult").SetValue(null, Engine.TimeMult * 0.2f, null);
            orig(self);
            typeof(Engine).GetProperty("TimeMult").SetValue(null, Engine.TimeMult / 0.2f, null);
            return;
        }
        if (matchVariants.GetCustomVariant("AwfullyFastArrows")) 
        {
            typeof(Engine).GetProperty("TimeMult").SetValue(null, Engine.TimeMult * 3.0f, null);
            orig(self);
            typeof(Engine).GetProperty("TimeMult").SetValue(null, Engine.TimeMult / 3.0f, null);
            return;
        }
        orig(self);
    }
}