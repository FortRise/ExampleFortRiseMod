using FortRise;
using Monocle;
using TowerFall;

namespace Teuria.AdditionalVariants;

internal static class TfStateInterop
{
    public static bool IsAvailable => TfStateAPIModImports.RegisterVariantStateEvents is not null;

    public static Level? CurrentLevel => Engine.Instance?.Scene as Level;

    public static bool ShouldFreezeCosmetics => TfStateAPIModImports.ShouldFreezeCosmetics?.Invoke() ?? false;

    public static void RegisterRng() => TfStateAPIModImports.RegisterRng?.Invoke();

    public static void UnregisterRng() => TfStateAPIModImports.UnregisterRng?.Invoke();

    //Those work by default already
    private static readonly string[] StatelessVariants =
    [
        "BottomlessQuiver",
        "ChestDeath",
        "AtomicArrow",
        "ShockDeath",
        "DarkWorld",
        "LavaOverload",
        "NoHypers",
        "NoDodgeCancel",
        "NeonArena",
        "KingsWrath",
        "NoArrowTinks",
        "UnfairAutobalance",
        "AutoOpenChest",
        "ExplodingShield",
        "ClumsySwap",
        "NoExplosionDamage",
        "FragilePrism",
    ];

    public static void Register(Mod module)
    {
        var register = TfStateAPIModImports.RegisterVariantStateEvents;
        if (register is null)
        {
            return;
        }

        register(module, JesterHatState.Name, JesterHatState.OnSaveState, JesterHatState.OnLoadState);
        register(module, DashStaminaState.Name, DashStaminaState.OnSaveState, DashStaminaState.OnLoadState);
        register(module, DrillingArrowState.Name, DrillingArrowState.OnSaveState, DrillingArrowState.OnLoadState);
        register(module, FadingArrowState.Name, FadingArrowState.OnSaveState, FadingArrowState.OnLoadState);
        register(module, AnnoyingMageState.Name, AnnoyingMageState.OnSaveState, AnnoyingMageState.OnLoadState);

        foreach (var variant in StatelessVariants)
        {
            register(module, variant, static () => [], static _ => { });
        }

        //ChaoticRoll can't work, at least not now: it play with MatchSettings.Variants
        //which TF.State assume wont change after starting the session
    }
}
