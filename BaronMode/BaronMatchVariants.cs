using FortRise;

namespace Teuria.BaronMode;

public class BaronMatchVariants : IRegisterable
{
    public static IVariantEntry NoTreasureLives = null!;

    public static void Register(IModContent content, IModRegistry registry)
    {
        NoTreasureLives = registry.Variants.RegisterVariant("TreasureLives", new() 
        {
            Title = "NO TREASURE LIVES",
            Flags = CustomVariantFlags.CanRandom,
            Icon = registry.Subtextures.RegisterTexture(
                content.Root.GetRelativePath("Content/variants/treasureLives.png")
            )
        });
    }
}
