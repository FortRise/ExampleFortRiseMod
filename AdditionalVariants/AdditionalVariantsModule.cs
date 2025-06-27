using System;
using FortRise;
using Microsoft.Extensions.Logging;

namespace Teuria.AdditionalVariants;

public sealed class AdditionalVariantsModule : Mod
{
    public static AdditionalVariantsModule Instance { get; private set; } = null!;
    public static IEffectEntry NeonShader { get; private set; } = null!;
    private static Type[] Registerables = [
        typeof(TextureRegistry),
        typeof(Variants),
        typeof(InvincibleTechnomage)
    ];

    private static Type[] Hookables = [
        typeof(AtomicArrow),
        typeof(AutoOpenChest),
        typeof(BottomlessQuiver),
        typeof(ChaoticRoll),
        typeof(ClumsySwap),
        typeof(DarkWorld),
        typeof(DrillingArrow),
        typeof(ExplodingShield),
        typeof(InvincibleTechnomageVariantSequence),
        typeof(FadingArrow),
        typeof(NeonFilter),
        typeof(JesterHat),
        typeof(KingsWrath),
        typeof(LavaOverload),
        typeof(NoArrowTinks),
        typeof(NoDodgeCancel),
        typeof(NoExplosionDamage),
        typeof(PlayerDeathVariants),
        typeof(PlayerStamina),
        typeof(UnfairAutobalance),
    ];

    public AdditionalVariantsModule(IModContent content, IModuleContext context, ILogger logger) : base(content, context, logger)
    {
        Instance = this;
        foreach (var hookable in Hookables)
        {
            hookable.GetMethod(nameof(IHookable.Load))!.Invoke(null, [context.Harmony]);
        }

        foreach (var registerable in Registerables)
        {
            registerable.GetMethod(nameof(IRegisterable.Register))!.Invoke(null, [content, context.Registry]);
        }

        NeonShader = context.Registry.Effects.RegisterEffect("NeonFX", new()
        {
            EffectResourceType = typeof(NeonShaderResource),
            EffectFile = content.Root.GetRelativePath("Content/Effects/neon.fxb"),
            PassName = "Neon"
        });
    }

    public override object? GetApi()
    {
        return new ApiImplementation();
    }
}