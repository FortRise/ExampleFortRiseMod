using FortRise;
using TowerFall;

namespace Teuria.BaronMode;

public class BaronMatchVariants : IRegisterable
{
    public static IVariantEntry TreasureLivesInfo = null!;

    public static void Register(IModRegistry registry)
    {
        TreasureLivesInfo = registry.Variants.RegisterVariant("TreasureLives", new() 
        {
            Title = "TREASURE LIVES",
            Description = "SPAWNS A TREASURE THAT ADDS A LIVES BY ONE",
            Flags = CustomVariantFlags.None,
            Icon = TFGame.MenuAtlas["Teuria.BaronMode/variants/treasureLives"]
        });
    }
}