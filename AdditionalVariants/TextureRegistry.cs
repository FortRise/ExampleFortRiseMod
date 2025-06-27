using FortRise;
using Monocle;
using TowerFall;

namespace Teuria.AdditionalVariants;

// Optional way to use textures
public class TextureRegistry : IRegisterable
{
    public const string MetaName = "Teuria.AdditionalVariants";
    // Variants
    public static ISubtextureEntry BottomlessQuiver { get; private set; } = null!;
    public static ISubtextureEntry AtomicArrow { get; private set; } = null!;
    public static ISubtextureEntry AnnoyingMage { get; private set; } = null!;
    public static ISubtextureEntry ShockDeath { get; private set; } = null!;
    public static ISubtextureEntry ChestDeath { get; private set; } = null!;
    public static ISubtextureEntry JesterHat { get; private set; } = null!;
    public static ISubtextureEntry DarkWorld { get; private set; } = null!;
    public static ISubtextureEntry LavaOverload { get; private set; } = null!;
    public static ISubtextureEntry ChaoticRoll { get; private set; } = null!;
    public static ISubtextureEntry NoHypers { get; private set; } = null!;
    public static ISubtextureEntry NoDodgeCancel { get; private set; } = null!;
    public static ISubtextureEntry FadingArrow { get; private set; } = null!;
    public static ISubtextureEntry NeonWorld { get; private set; } = null!;
    public static ISubtextureEntry DashStamina { get; private set; } = null!;
    public static ISubtextureEntry KingsWrath { get; private set; } = null!;
    public static ISubtextureEntry NoArrowTinks { get; private set; } = null!;
    public static ISubtextureEntry DrillingArrow { get; private set; } = null!;
    public static ISubtextureEntry UnfairAutobalance { get; private set; } = null!;
    public static ISubtextureEntry ArrowFallingUp { get; private set; } = null!;
    public static ISubtextureEntry AutoOpenChest { get; private set; } = null!;
    public static ISubtextureEntry ExplodingShield { get; private set; } = null!;
    public static ISubtextureEntry ClumsySwap { get; private set; } = null!;
    public static ISubtextureEntry NoExplosionDamage { get; private set; } = null!;

    // Chest
    public static ISubtextureEntry GrayChest { get; private set; } = null!;
    public static ISubtextureEntry BlueChest { get; private set; } = null!;
    public static ISubtextureEntry RedChest { get; private set; } = null!;

    // Misc
    public static ISubtextureEntry StaminaBar { get; private set; } = null!;

    public static void Register(IModContent content, IModRegistry registry)
    {
        BottomlessQuiver = registry.Subtextures.RegisterTexture("BottomlessQuiver", content.Root.GetRelativePath("Content/images/variants/bottomlessQuiver.png"));
        AtomicArrow = registry.Subtextures.RegisterTexture("AtomicArrow", content.Root.GetRelativePath("Content/images/variants/atomicArrow.png"));
        AnnoyingMage = registry.Subtextures.RegisterTexture("AnnoyingMage", content.Root.GetRelativePath("Content/images/variants/annoyingMage.png"));
        ShockDeath = registry.Subtextures.RegisterTexture("ShockDeath", content.Root.GetRelativePath("Content/images/variants/shockDeath.png"));
        ChestDeath = registry.Subtextures.RegisterTexture("ChestDeath", content.Root.GetRelativePath("Content/images/variants/chestDeath.png"));
        JesterHat = registry.Subtextures.RegisterTexture("JesterHat", content.Root.GetRelativePath("Content/images/variants/jesterHat.png"));
        DarkWorld = registry.Subtextures.RegisterTexture("DarkWorld", content.Root.GetRelativePath("Content/images/variants/darkWorld.png"));
        LavaOverload = registry.Subtextures.RegisterTexture("LavaOverload", content.Root.GetRelativePath("Content/images/variants/lavaOverload.png"));
        ChaoticRoll = registry.Subtextures.RegisterTexture("ChaoticRoll", content.Root.GetRelativePath("Content/images/variants/chaoticroll.png"));
        NoHypers = registry.Subtextures.RegisterTexture("NoHypers", content.Root.GetRelativePath("Content/images/variants/noHypers.png"));
        NoDodgeCancel = registry.Subtextures.RegisterTexture("NoDodgeCancel", content.Root.GetRelativePath("Content/images/variants/noDodgeCancel.png"));
        FadingArrow = registry.Subtextures.RegisterTexture("FadingArrow", content.Root.GetRelativePath("Content/images/variants/fadingArrow.png"));
        NeonWorld = registry.Subtextures.RegisterTexture("NeonWorld", content.Root.GetRelativePath("Content/images/variants/neonWorld.png"));
        DashStamina = registry.Subtextures.RegisterTexture("DashStamina", content.Root.GetRelativePath("Content/images/variants/dashStamina.png"));
        KingsWrath = registry.Subtextures.RegisterTexture("KingsWrath", content.Root.GetRelativePath("Content/images/variants/kingsWrath.png"));
        NoArrowTinks = registry.Subtextures.RegisterTexture("NoArrowTinks", content.Root.GetRelativePath("Content/images/variants/noArrowTinks.png"));
        DrillingArrow = registry.Subtextures.RegisterTexture("DrillingArrow", content.Root.GetRelativePath("Content/images/variants/drillingArrow.png"));
        UnfairAutobalance = registry.Subtextures.RegisterTexture("UnfairAutobalance", content.Root.GetRelativePath("Content/images/variants/unfairAutobalance.png"));
        ArrowFallingUp = registry.Subtextures.RegisterTexture("ArrowFallingUp", content.Root.GetRelativePath("Content/images/variants/arrowFallingUp.png"));
        AutoOpenChest = registry.Subtextures.RegisterTexture("AutoOpenChest", content.Root.GetRelativePath("Content/images/variants/autoOpenChest.png"));
        ExplodingShield = registry.Subtextures.RegisterTexture("ExplodingShield", content.Root.GetRelativePath("Content/images/variants/explodingShield.png"));
        ClumsySwap = registry.Subtextures.RegisterTexture("ClumsySwap", content.Root.GetRelativePath("Content/images/variants/clumsySwap.png"));
        NoExplosionDamage = registry.Subtextures.RegisterTexture("NoExplosionDamage", content.Root.GetRelativePath("Content/images/variants/noExplosionDamage.png"));

        GrayChest = registry.Subtextures.RegisterTexture("GrayChest", content.Root.GetRelativePath("Content/images/chest/graychest.png"));
        BlueChest = registry.Subtextures.RegisterTexture("BlueChest", content.Root.GetRelativePath("Content/images/chest/bluechest.png"));
        RedChest = registry.Subtextures.RegisterTexture("RedChest", content.Root.GetRelativePath("Content/images/chest/redchest.png"));

        StaminaBar = registry.Subtextures.RegisterTexture("StaminaBar", content.Root.GetRelativePath("Content/images/misc/staminabar.png"));
    }
}