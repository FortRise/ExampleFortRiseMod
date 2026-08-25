using System;
using System.Linq;
using Monocle;
using MonoMod.Utils;
using TowerFall;

namespace Teuria.AdditionalVariants;

internal static class FadingArrowState
{
    public const string Name = "FadingArrow";

    // TF.State already handle vanilla flash state
    public static byte[] OnSaveState() => [];

    // Only the completion callback is ours here and a delegate cannot be serialized
    public static void OnLoadState(byte[] state)
    {
        var arrows = TfStateInterop.CurrentLevel?[GameTags.Arrow];
        if (arrows is null)
        {
            return;
        }

        foreach (var arrow in arrows.OfType<Arrow>())
        {
            if (arrow is LaserArrow || !arrow.Flashing || arrow.PlayerIndex < 0)
            {
                continue;
            }

            if (!Variants.FadingArrow.IsActive(arrow.PlayerIndex))
            {
                continue;
            }

            var fading = arrow;
            DynamicData.For(arrow).Set("onFinish", (Action)(() =>
            {
                fading.StopFlashing();
                fading.RemoveSelf();
            }));
        }
    }
}
