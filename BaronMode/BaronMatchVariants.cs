using FortRise;

namespace Teuria.BaronMode;

public class BaronMatchVariants : IRegisterable
{
    public static IVariantEntry TreasureLivesInfo = null!;

    public static void Register(IModContent content, IModRegistry registry)
    {
        TreasureLivesInfo = registry.Variants.RegisterVariant("TreasureLives", new() 
        {
            Title = "TREASURE LIVES",
            Description = "SPAWNS A TREASURE THAT ADDS A LIVES BY ONE",
            Flags = CustomVariantFlags.None,
            Icon = registry.Subtextures.RegisterTexture(
                content.Root.GetRelativePath("Content/variants/treasureLives.png")
            )
        });
    }
}