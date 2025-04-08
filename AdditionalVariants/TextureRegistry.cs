using Monocle;
using TowerFall;

namespace Teuria.AdditionalVariants;

// Optional way to use textures
public static class TextureRegistry 
{
    public const string MetaName = "Teuria.AdditionalVariants";
    // Variants
    public static Subtexture BottomlessQuiver => TFGame.Atlas["Teuria.AdditionalVariants/variants/bottomlessQuiver"];
    public static Subtexture AtomicArrow => TFGame.Atlas["Teuria.AdditionalVariants/variants/atomicArrow"];
    public static Subtexture AnnoyingMage => TFGame.Atlas["Teuria.AdditionalVariants/variants/annoyingMage"];
    public static Subtexture ShockDeath => TFGame.Atlas["Teuria.AdditionalVariants/variants/shockDeath"];
    public static Subtexture ChestDeath => TFGame.Atlas["Teuria.AdditionalVariants/variants/chestDeath"];
    public static Subtexture JesterHat => TFGame.Atlas["Teuria.AdditionalVariants/variants/jesterHat"];
    public static Subtexture DarkWorld => TFGame.Atlas["Teuria.AdditionalVariants/variants/darkWorld"];
    public static Subtexture LavaOverload => TFGame.Atlas["Teuria.AdditionalVariants/variants/lavaOverload"];
    public static Subtexture ChaoticRoll => TFGame.Atlas["Teuria.AdditionalVariants/variants/chaoticroll"];
    public static Subtexture NoHypers => TFGame.Atlas["Teuria.AdditionalVariants/variants/noHypers"];
    public static Subtexture NoDodgeCancel => TFGame.Atlas["Teuria.AdditionalVariants/variants/noDodgeCancel"];
    public static Subtexture FadingArrow => TFGame.Atlas["Teuria.AdditionalVariants/variants/fadingArrow"];
    public static Subtexture NeonWorld => TFGame.Atlas["Teuria.AdditionalVariants/variants/neonWorld"];
    public static Subtexture DashStamina => TFGame.Atlas["Teuria.AdditionalVariants/variants/dashStamina"];
    public static Subtexture KingsWrath => TFGame.Atlas["Teuria.AdditionalVariants/variants/kingsWrath"];
    public static Subtexture NoArrowTinks => TFGame.Atlas["Teuria.AdditionalVariants/variants/noArrowTinks"];
    public static Subtexture DrillingArrow => TFGame.Atlas["Teuria.AdditionalVariants/variants/drillingArrow"];
    public static Subtexture UnfairAutobalance => TFGame.Atlas["Teuria.AdditionalVariants/variants/unfairAutobalance"];
    public static Subtexture ArrowFallingUp => TFGame.Atlas["Teuria.AdditionalVariants/variants/arrowFallingUp"];
    public static Subtexture AutoOpenChest => TFGame.Atlas["Teuria.AdditionalVariants/variants/autoOpenChest"];
    public static Subtexture ExplodingShield => TFGame.Atlas["Teuria.AdditionalVariants/variants/explodingShield"];
    public static Subtexture ClumsySwap => TFGame.Atlas["Teuria.AdditionalVariants/variants/clumsySwap"];
    public static Subtexture NoExplosionDamage => TFGame.Atlas["Teuria.AdditionalVariants/variants/noExplosionDamage"];

    // Chest
    public static Subtexture GrayChest => TFGame.Atlas["Teuria.AdditionalVariants/chest/graychest"];
    public static Subtexture BlueChest => TFGame.Atlas["Teuria.AdditionalVariants/chest/bluechest"];
    public static Subtexture RedChest => TFGame.Atlas["Teuria.AdditionalVariants/chest/redchest"];


    // Misc
    public static Subtexture StaminaBar => TFGame.Atlas["Teuria.AdditionalVariants/misc/staminabar"];
}