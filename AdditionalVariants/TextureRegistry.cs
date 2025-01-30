using Monocle;
using TowerFall;

namespace AdditionalVariants;

// Optional way to use textures
public static class TextureRegistry 
{
    // Variants
    public static Subtexture BottomlessQuiver => TFGame.Atlas["AdditionalVariants/variants/bottomlessQuiver"];
    public static Subtexture AtomicArrow => TFGame.Atlas["AdditionalVariants/variants/atomicArrow"];
    public static Subtexture AnnoyingMage => TFGame.Atlas["AdditionalVariants/variants/annoyingMage"];
    public static Subtexture ShockDeath => TFGame.Atlas["AdditionalVariants/variants/shockDeath"];
    public static Subtexture ChestDeath => TFGame.Atlas["AdditionalVariants/variants/chestDeath"];
    public static Subtexture JesterHat => TFGame.Atlas["AdditionalVariants/variants/jesterHat"];
    public static Subtexture DarkWorld => TFGame.Atlas["AdditionalVariants/variants/darkWorld"];
    public static Subtexture LavaOverload => TFGame.Atlas["AdditionalVariants/variants/lavaOverload"];
    public static Subtexture ChaoticRoll => TFGame.Atlas["AdditionalVariants/variants/chaoticroll"];
    public static Subtexture NoHypers => TFGame.Atlas["AdditionalVariants/variants/noHypers"];
    public static Subtexture NoDodgeCancel => TFGame.Atlas["AdditionalVariants/variants/noDodgeCancel"];
    public static Subtexture FadingArrow => TFGame.Atlas["AdditionalVariants/variants/fadingArrow"];
    public static Subtexture NeonWorld => TFGame.Atlas["AdditionalVariants/variants/neonWorld"];
    public static Subtexture DashStamina => TFGame.Atlas["AdditionalVariants/variants/dashStamina"];
    public static Subtexture KingsWrath => TFGame.Atlas["AdditionalVariants/variants/kingsWrath"];
    public static Subtexture NoArrowTinks => TFGame.Atlas["AdditionalVariants/variants/noArrowTinks"];

    // Chest
    public static Subtexture GrayChest => TFGame.Atlas["AdditionalVariants/chest/graychest"];
    public static Subtexture BlueChest => TFGame.Atlas["AdditionalVariants/chest/bluechest"];
    public static Subtexture RedChest => TFGame.Atlas["AdditionalVariants/chest/redchest"];

    // Misc
    public static Subtexture StaminaBar => TFGame.Atlas["AdditionalVariants/misc/staminabar"];
}