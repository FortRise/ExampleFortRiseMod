using System;
using AdditionalVariants.EX;
using AdditionalVariants.EX.JesterHat;
using FortRise;

namespace Teuria.AdditionalVariants;

public class AdditionalVariantsModule : FortModule
{
    public static NeonShaderResource NeonShader = null!;

    private static Type[] Registerables = [
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

    public override void LoadContent()
    {
        NeonShader = Content.LoadShader<NeonShaderResource>("Effects/neon.fxb", "Neon", out var _);
    }

    public override void Load()
    {
        foreach (var hookable in Hookables)
        {
            hookable.GetMethod(nameof(IHookable.Load))!.Invoke(null, []);
        }
        // ArrowFallingUp.Load();
        FortRise.RiseCore.Events.OnPreInitialize += OnPreInitialize;
    }

    public override void Unload()
    {

        foreach (var hookable in Hookables)
        {
            hookable.GetMethod(nameof(IHookable.Unload))!.Invoke(null, []);
        }
        // ArrowFallingUp.Unload();
        FortRise.RiseCore.Events.OnPreInitialize -= OnPreInitialize;
    }

    public override void Initialize()
    {
        foreach (var registerable in Registerables)
        {
            registerable.GetMethod(nameof(IRegisterable.Register))!.Invoke(null, [Registry]);
        }
    }

    private void OnPreInitialize()
    {
        TfExAPIModImports.RegisterVariantStateEvents?.Invoke
            (this, "JestersHat", JesterHatStateEvents.OnSaveState, JesterHatStateEvents.OnLoadState);
    }
}
