using System.Reflection;
using FortRise;
using Monocle;
using MonoMod.Utils;
using TowerFall;

namespace BartizanMod;

public class MyPlayer 
{
    private static IVariantEntry NoHeadBounce { get; set; } = null!;
    private static IVariantEntry NoDodgeCooldown { get; set; } = null!;
    private static IVariantEntry NoLedgeGrab { get; set; } = null!;
    private static IVariantEntry InfiniteArrows { get; set; } = null!;

    internal static void Register(IModRegistry registry)
    {
        var showDodgeCooldown = registry.Variants.GetVariant("ShowDodgeCooldown")!;

        NoHeadBounce = registry.Variants.RegisterVariant("NoHeadBounce", new() 
        {
            Title = "NO HEADBOUNCE",
            Icon = TFGame.MenuAtlas["Kha.Bartizan/variants/noHeadBounce"]
        });

        NoDodgeCooldown = registry.Variants.RegisterVariant("NoDodgeCooldowns", new() 
        {
            Title = "NO DODGE COOLDOWNS",
            Icon = TFGame.MenuAtlas["Kha.Bartizan/variants/noDodgeCooldowns"],
            Links = [showDodgeCooldown]
        });

        NoLedgeGrab = registry.Variants.RegisterVariant("NoLedgeGrab", new() 
        {
            Title = "NO LEDGE GRAB",
            Icon = TFGame.MenuAtlas["Kha.Bartizan/variants/noLedgeGrab"]
        });

        InfiniteArrows = registry.Variants.RegisterVariant("InfiniteArrows", new() 
        {
            Title = "INFINITE ARROWS",
            Icon = TFGame.MenuAtlas["Kha.Bartizan/variants/infiniteArrows"],
        });
    }

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
        if (NoHeadBounce.IsActive(self.PlayerIndex))
        {
            return;
        }

        orig(self, bouncerIndex);
    }

    public delegate bool orig_Player_CanGrabLedge(Player self, int targetY, int direction);

    public static bool Player_CanGrabLedge_patch(On.TowerFall.Player.orig_CanGrabLedge orig, Player self, int targetY, int direction) 
    {
        if (NoLedgeGrab.IsActive(self.PlayerIndex)) 
            return false;
        
        return orig(self, targetY, direction);
    }

    public static int Player_GetDodgeExitState(On.TowerFall.Player.orig_GetDodgeExitState orig, Player self) 
    {
        /* New */
        if (NoDodgeCooldown.IsActive(self.PlayerIndex)) 
        {
            var dynData = new DynData<Player>(self);
            dynData.Set("dodgeCooldown", false);
            dynData.Dispose();
        }
        return orig(self);
    }

    public static void Player_ShootArrow(On.TowerFall.Player.orig_ShootArrow orig, Player self) 
    {
        if (InfiniteArrows.IsActive(self.PlayerIndex)) 
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
    private static PropertyInfo Comp_TimeMult = null!;

    private static IVariantEntry AwfullyFastArrows { get; set; } = null!;
    private static IVariantEntry AwfullySlowArrows { get; set; } = null!;

    internal static void Register(IModRegistry registry)
    {
        AwfullyFastArrows = registry.Variants.RegisterVariant("AwfullyFastArrows", new() 
        {
            Title = "NO DODGE COOLDOWNS",
            Icon = TFGame.MenuAtlas["Kha.Bartizan/variants/awfullyFastArrows"]
        });

        AwfullySlowArrows = registry.Variants.RegisterVariant("AwfullySlowArrows", new() 
        {
            Title = "NO DODGE COOLDOWNS",
            Icon = TFGame.MenuAtlas["Kha.Bartizan/variants/awfullySlowArrows"],
            Links = [AwfullyFastArrows]
        });
    }

    internal static void Load() 
    {
        On.TowerFall.Arrow.ArrowUpdate += ArrowUpdate_patch;
        Comp_TimeMult = typeof(Engine).GetProperty("TimeMult")!;
    }

    internal static void Unload() 
    {
        On.TowerFall.Arrow.ArrowUpdate -= ArrowUpdate_patch;
    }

    private static void ArrowUpdate_patch(On.TowerFall.Arrow.orig_ArrowUpdate orig, Arrow self)
    {
        if (AwfullySlowArrows.IsActive(self.PlayerIndex)) 
        {
            Comp_TimeMult.SetValue(null, Engine.TimeMult * 0.2f, null);
            orig(self);
            Comp_TimeMult.SetValue(null, Engine.TimeMult / 0.2f, null);
            return;
        }
        if (AwfullyFastArrows.IsActive(self.PlayerIndex)) 
        {
            Comp_TimeMult.SetValue(null, Engine.TimeMult * 3.0f, null);
            orig(self);
            Comp_TimeMult.SetValue(null, Engine.TimeMult / 3.0f, null);
            return;
        }
        orig(self);
    }
}