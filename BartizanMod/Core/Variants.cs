using System.Reflection;
using FortRise;
using HarmonyLib;
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

    internal static void Register(IHarmony harmony, IModContent content, IModRegistry registry)
    {
        var showDodgeCooldown = registry.Variants.GetVariant("ShowDodgeCooldown")!;

        NoHeadBounce = registry.Variants.RegisterVariant("NoHeadBounce", new()
        {
            Title = "NO HEADBOUNCE",
            Icon = registry.Subtextures.RegisterTexture(
                content.Root.GetRelativePath("Content/variants/noHeadBounce.png")
            )
        });

        NoDodgeCooldown = registry.Variants.RegisterVariant("NoDodgeCooldowns", new()
        {
            Title = "NO DODGE COOLDOWNS",
            Icon = registry.Subtextures.RegisterTexture(
                content.Root.GetRelativePath("Content/variants/noDodgeCooldowns.png")
            ),
            Links = [showDodgeCooldown]
        });

        NoLedgeGrab = registry.Variants.RegisterVariant("NoLedgeGrab", new()
        {
            Title = "NO LEDGE GRAB",
            Icon = registry.Subtextures.RegisterTexture(
                content.Root.GetRelativePath("Content/variants/noLedgeGrab.png")
            )
        });

        InfiniteArrows = registry.Variants.RegisterVariant("InfiniteArrows", new()
        {
            Title = "INFINITE ARROWS",
            Icon = registry.Subtextures.RegisterTexture(
                content.Root.GetRelativePath("Content/variants/infiniteArrows.png")
            ),
        });

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Player), "ShootArrow"),
            new HarmonyMethod(Player_ShootArrow_Prefix),
            postfix: new HarmonyMethod(Player_ShootArrow_Postfix)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Player), "GetDodgeExitState"),
            new HarmonyMethod(Player_GetDodgeExitState_Prefix)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Player), "CanGrabLedge"),
            new HarmonyMethod(Player_CanGrabLedge_Prefix)
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Player), nameof(Player.HurtBouncedOn)),
            new HarmonyMethod(Player_HurtBouncedOn_Prefix)
        );
    }

    private static bool Player_HurtBouncedOn_Prefix(Player __instance)
    {
        if (NoHeadBounce.IsActive(__instance.PlayerIndex))
        {
            return false;
        }

        return true;
    }

    public static bool Player_CanGrabLedge_Prefix(Player __instance, ref bool __result) 
    {
        if (NoLedgeGrab.IsActive(__instance.PlayerIndex))
        {
            return __result = false;
        }

        return true;
    }

    public static void Player_GetDodgeExitState_Prefix(Player __instance) 
    {
        if (NoDodgeCooldown.IsActive(__instance.PlayerIndex)) 
        {
            var dynData = new DynData<Player>(__instance);
            dynData.Set("dodgeCooldown", false);
            dynData.Dispose();
        }
    }

    public static void Player_ShootArrow_Prefix(Player __instance, ref ArrowTypes __state) 
    {
        if (InfiniteArrows.IsActive(__instance.PlayerIndex)) 
        {
            __state = __instance.Arrows.Arrows[0];
        }
    }

    public static void Player_ShootArrow_Postfix(Player __instance, in ArrowTypes __state) 
    {
        if (InfiniteArrows.IsActive(__instance.PlayerIndex)) 
        {
            __instance.Arrows.AddArrows(__state);
        }
    }
}

public class MyArrow 
{
    private static PropertyInfo Comp_TimeMult = null!;

    private static IVariantEntry AwfullyFastArrows { get; set; } = null!;
    private static IVariantEntry AwfullySlowArrows { get; set; } = null!;

    internal static void Register(IHarmony harmony, IModContent content, IModRegistry registry)
    {
        AwfullyFastArrows = registry.Variants.RegisterVariant("AwfullyFastArrows", new()
        {
            Title = "AWFULLY FAST ARROWS",
            Icon = registry.Subtextures.RegisterTexture(
                content.Root.GetRelativePath("Content/variants/awfullyFastArrows.png")
            )
        });

        AwfullySlowArrows = registry.Variants.RegisterVariant("AwfullySlowArrows", new()
        {
            Title = "AWFULLY SLOW ARROWS",
            Icon = registry.Subtextures.RegisterTexture(
                content.Root.GetRelativePath("Content/variants/awfullySlowArrows.png")
            ),
            Links = [AwfullyFastArrows]
        });

        Comp_TimeMult = typeof(Engine).GetProperty("TimeMult")!;

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Arrow), nameof(Arrow.ArrowUpdate)),
            prefix: new HarmonyMethod(Arrow_ArrowUpdate_Prefix),
            postfix: new HarmonyMethod(Arrow_ArrowUpdate_Postfix)
        );
    }

    private static void Arrow_ArrowUpdate_Prefix(Arrow __instance)
    {
        if (AwfullySlowArrows.IsActive(__instance.PlayerIndex)) 
        {
            Comp_TimeMult.SetValue(null, Engine.TimeMult * 0.2f, null);
        }
        else if (AwfullyFastArrows.IsActive(__instance.PlayerIndex)) 
        {
            Comp_TimeMult.SetValue(null, Engine.TimeMult * 3.0f, null);
        }
    }

    private static void Arrow_ArrowUpdate_Postfix(Arrow __instance)
    {
        if (AwfullySlowArrows.IsActive(__instance.PlayerIndex))
        {
            Comp_TimeMult.SetValue(null, Engine.TimeMult / 0.2f, null);
        }
        else if (AwfullyFastArrows.IsActive(__instance.PlayerIndex))
        {
            Comp_TimeMult.SetValue(null, Engine.TimeMult / 3.0f, null);
        }
    }
}