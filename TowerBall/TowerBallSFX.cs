using FortRise;

namespace TowerBall;

public static class TowerBallSFX
{
    public static ISFXEntry BallSFX { get; private set; } = null!;

    public static void Register(IModContent content, IModuleContext context)
    {
        BallSFX = context.Registry.SFXs.RegisterSFX(
            "Ball", 
            content.Root.GetRelativePath("Content/SFX/BOUNCYBALL.wav")
        );
    }
}
