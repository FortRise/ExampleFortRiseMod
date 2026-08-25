using System;
using FortRise;
using MonoMod.ModInterop;

namespace Teuria.AdditionalVariants;

[ModImportName("TF.State.API")]
public static class TfStateAPIModImports
{
    public static Action<Mod, string, Func<byte[]>, Action<byte[]>>? RegisterVariantStateEvents;
    public static Action<Mod, string>? UnregisterVariantStateEvents;
    public static Func<bool>? ShouldFreezeCosmetics;
    public static Action? RegisterRng;
    public static Action? UnregisterRng;

    static TfStateAPIModImports()
    {
        typeof(TfStateAPIModImports).ModInterop();
    }
}
