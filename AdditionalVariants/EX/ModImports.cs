using System;
using FortRise;
using MonoMod.ModInterop;

namespace AdditionalVariants.EX
{
    [ModImportName("TF.EX.API")]
    public static class TfExAPIModImports
    {
        public static Action<FortModule, string, Func<string>, Action<string>> RegisterVariantStateEvents = null!;

        static TfExAPIModImports()
        {
            typeof(TfExAPIModImports).ModInterop();
        }
    }
}
