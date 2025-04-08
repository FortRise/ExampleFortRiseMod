using FortRise;

namespace Teuria.AdditionalVariants;

public class Variants : IRegisterable
{
    public static IVariant BottomlessQuiver { get; internal set; } = null!;
    public static IVariant AtomicArrow { get; internal set; } = null!;
    public static IVariant AnnoyingMage { get; internal set; } = null!;
    public static IVariant ShockDeath { get; internal set; } = null!;
    public static IVariant ChestDeath { get; internal set; } = null!;
    public static IVariant JestersHat { get; internal set; } = null!;
    public static IVariant DarkWorld { get; internal set; } = null!;
    public static IVariant LavaOverload { get; internal set; } = null!;
    public static IVariant ChaoticRoll { get; internal set; } = null!;
    public static IVariant NoHypers { get; internal set; } = null!;
    public static IVariant NoDodgeCancel { get; internal set; } = null!;
    public static IVariant FadingArrow { get; internal set; } = null!;
    public static IVariant NeonWorld { get; internal set; } = null!;
    public static IVariant DashStamina { get; internal set; } = null!;
    public static IVariant KingsWrath { get; internal set; } = null!;
    public static IVariant NoArrowTinks { get; internal set; } = null!;
    public static IVariant DrillingArrow { get; internal set; } = null!;
    public static IVariant UnfairAutobalance { get; internal set; } = null!;
    public static IVariant AutoOpenChest { get; internal set; } = null!;
    public static IVariant ExplodingShield { get; internal set; } = null!;
    public static IVariant ClumsySwap { get; internal set; } = null!;
    public static IVariant NoExplosionDamage { get; internal set; } = null!;


    public static void Register(IModRegistry registry)
    {
        var smallQuivers = registry.Variants.GetVariant("SmallQuivers")!;
        var noQuivers = registry.Variants.GetVariant("NoQuivers")!;
        var darkPortals = registry.Variants.GetVariant("DarkPortals")!;
        var noDodging = registry.Variants.GetVariant("NoDodging")!;
        var alwaysDark = registry.Variants.GetVariant("AlwaysDark")!;
        var noAutobalance = registry.Variants.GetVariant("NoAutobalance")!;
        var weakAutobalance = registry.Variants.GetVariant("WeakAutobalance")!;
        var bombChests = registry.Variants.GetVariant("BombChests")!;
        var noTreasure = registry.Variants.GetVariant("NoTreasure")!;

        BottomlessQuiver = registry.Variants.RegisterVariant("BottomlessQuiver", new() 
        {
            Title = "BOTTOMLESS QUIVER",
            Icon = TextureRegistry.BottomlessQuiver,
            Description = "NO LIMIT ON HOW MANY ARROWS YOU CAN HOLD",
            Flags = CustomVariantFlags.PerPlayer | CustomVariantFlags.CanRandom,
            Links = [smallQuivers, noQuivers]
        });

        AtomicArrow = registry.Variants.RegisterVariant("AtomicArrow", new() 
        {
            Title = "ATOMIC ARROW",
            Icon = TextureRegistry.AtomicArrow,
            Description = "ARROWS EXPLODES WHEN IT HIT A WALL",
            Flags = CustomVariantFlags.PerPlayer | CustomVariantFlags.CanRandom

        });

        AnnoyingMage = registry.Variants.RegisterVariant("AnnoyingMage", new() 
        {
            Title = "ANNOYING MAGE",
            Icon = TextureRegistry.AnnoyingMage,
            Description = "SUMMONS AN INVINCIBLE TECHNOMAGE",
            Flags = CustomVariantFlags.CanRandom | CustomVariantFlags.DarkWorldDLC,
            Links = [darkPortals]
        });

        ShockDeath = registry.Variants.RegisterVariant("ShockDeath", new() 
        {
            Title = "SHOCK DEATH",
            Icon = TextureRegistry.ShockDeath,
            Description = "SUMMONS A SHOCKWAVE WHEN A PLAYER DIES",
            Flags = CustomVariantFlags.PerPlayer | CustomVariantFlags.CanRandom
        });

        ChestDeath = registry.Variants.RegisterVariant("ChestDeath", new() 
        {
            Title = "CHEST DEATH",
            Icon = TextureRegistry.ChestDeath,
            Description = "SPAWNS A CHEST WHEN A PLAYER DIES",
            Flags = CustomVariantFlags.PerPlayer | CustomVariantFlags.CanRandom
        });

        JestersHat = registry.Variants.RegisterVariant("JestersHat", new() 
        {
            Title = "JESTER'S HAT",
            Icon = TextureRegistry.JesterHat,
            Description = "ALLOWS PLAYERS TO TELEPORT BY DASHING",
            Flags = CustomVariantFlags.PerPlayer | CustomVariantFlags.CanRandom
        });

        DarkWorld = registry.Variants.RegisterVariant("DarkWorld", new() 
        {
            Title = "DARK WORLD",
            Icon = TextureRegistry.DarkWorld,
            Description = "MAKES THE DARK EFFECT DARKER",
            Flags =  CustomVariantFlags.CanRandom
        });

        LavaOverload = registry.Variants.RegisterVariant("LavaOverload", new() 
        {
            Title = "LAVA OVERLOAD",
            Icon = TextureRegistry.LavaOverload,
            Description = "LAVA WILL APPEAR ON ALL FOUR SIDES",
            Flags =  CustomVariantFlags.CanRandom
        });

        ChaoticRoll = registry.Variants.RegisterVariant("ChaoticRoll", new() 
        {
            Title = "CHAOTIC ROLL",
            Icon = TextureRegistry.ChaoticRoll,
            Description = "RANDOM VARIANTS EVERY ROUND"
        });

        NoHypers = registry.Variants.RegisterVariant("NoHypers", new() 
        {
            Title = "NO HYPERS",
            Icon = TextureRegistry.NoHypers,
            Description = "REMOVES THE ABILITY TO HYPER DODGE",
            Flags =  CustomVariantFlags.CanRandom | CustomVariantFlags.PerPlayer,
            Links = [noDodging]
        });

        NoDodgeCancel = registry.Variants.RegisterVariant("NoDodgeCancel", new() 
        {
            Title = "NO DODGE CANCEL",
            Icon = TextureRegistry.NoDodgeCancel,
            Description = "REMOVES THE DODGE CANCELLATION MECHANIC",
            Flags =  CustomVariantFlags.CanRandom | CustomVariantFlags.PerPlayer,
            Links = [NoHypers, noDodging]
        });

        FadingArrow = registry.Variants.RegisterVariant("FadingArrow", new() 
        {
            Title = "FADING ARROW",
            Icon = TextureRegistry.FadingArrow,
            Description = "ARROW WILL START FADING WHEN STUCK TO A WALL",
            Flags =  CustomVariantFlags.CanRandom | CustomVariantFlags.PerPlayer
        });

        NeonWorld = registry.Variants.RegisterVariant("NeonArena", new() 
        {
            Title = "NEON ARENA",
            Icon = TextureRegistry.NeonWorld,
            Description = "WELCOME TO THE CYBER WORLD",
            Flags =  CustomVariantFlags.CanRandom,
            Links = [alwaysDark]
        });

        DashStamina = registry.Variants.RegisterVariant("DashStamina", new() 
        {
            Title = "DASH STAMINA",
            Icon = TextureRegistry.DashStamina,
            Description = "ADDS A STAMINA MECHANIC FOR DASHES",
            Flags =  CustomVariantFlags.CanRandom | CustomVariantFlags.PerPlayer,
            Links = [noDodging]
        });

        KingsWrath = registry.Variants.RegisterVariant("KingsWrath", new() 
        {
            Title = "KING'S WRATH",
            Icon = TextureRegistry.KingsWrath,
            Description = "CROWN DROPS ON THE FLOOR WILL SPAWN LETHAL GHOST",
            Flags =  CustomVariantFlags.CanRandom | CustomVariantFlags.PerPlayer
        });

        NoArrowTinks = registry.Variants.RegisterVariant("NoArrowTinks", new() 
        {
            Title = "NO ARROW TINKS",
            Icon = TextureRegistry.NoArrowTinks,
            Description = "DISABLED COLLISION OF AN ARROW TO ARROW",
            Flags =  CustomVariantFlags.CanRandom 
        });

        DrillingArrow = registry.Variants.RegisterVariant("DrillingArrow", new() 
        {
            Title = "DRILLING ARROW",
            Icon = TextureRegistry.DrillingArrow,
            Description = "MAKE ALL ARROWS ACT LIKE A DRILL ARROW",
            Flags =  CustomVariantFlags.CanRandom | CustomVariantFlags.PerPlayer
        });

        UnfairAutobalance = registry.Variants.RegisterVariant("UnfairAutobalance", new() 
        {
            Title = "UNFAIR AUTOBALANCE",
            Icon = TextureRegistry.UnfairAutobalance,
            Description = "TOP PLAYERS ALWAYS GET AN ADVANTAGE",
            Flags =  CustomVariantFlags.CanRandom,
            Links = [weakAutobalance, noAutobalance]
        });

        AutoOpenChest = registry.Variants.RegisterVariant("AutoOpenChest", new() 
        {
            Title = "AUTO OPEN CHEST",
            Icon = TextureRegistry.AutoOpenChest,
            Description = "CHEST OPENS ON LAND",
            Flags =  CustomVariantFlags.CanRandom,
            Links = [bombChests, noTreasure]
        });

        ExplodingShield = registry.Variants.RegisterVariant("ExplodingShield", new() 
        {
            Title = "EXPLODING SHIELD",
            Icon = TextureRegistry.ExplodingShield,
            Description = "SHIELD EXPLODES ON BREAK",
            Flags =  CustomVariantFlags.CanRandom | CustomVariantFlags.PerPlayer
        });

        ClumsySwap = registry.Variants.RegisterVariant("ClumsySwap", new() 
        {
            Title = "CLUMSY SWAP",
            Icon = TextureRegistry.ClumsySwap,
            Description = "SWAPPING CAUSES AN ARROW TO DROP",
            Flags =  CustomVariantFlags.CanRandom | CustomVariantFlags.PerPlayer
        });

        NoExplosionDamage = registry.Variants.RegisterVariant("NoExplosionDamage", new() 
        {
            Title = "NO EXPLOSION DAMAGE",
            Icon = TextureRegistry.NoExplosionDamage,
            Description = "EXPLOSION WILL NOT KILL ANYONE",
            Flags =  CustomVariantFlags.CanRandom | CustomVariantFlags.PerPlayer
        });
    }
}