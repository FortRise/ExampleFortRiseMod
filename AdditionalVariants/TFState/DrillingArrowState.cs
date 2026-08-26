using Monocle;
using MonoMod.Utils;
using System.Collections.Generic;
using System.Linq;
using TowerFall;

namespace Teuria.AdditionalVariants;

internal static class DrillingArrowState
{
    public const string Name = "DrillingArrow";

    public static byte[] OnSaveState()
    {
        var arrows = TfStateInterop.CurrentLevel?[GameTags.Arrow];
        if (arrows is null)
        {
            return [];
        }

        var drilled = arrows
            .OfType<Arrow>()
            .Where(arrow => arrow.HasDrilled || arrow.NaivePush)
            .Select(arrow => (Arrow: arrow, Depth: DynamicData.For(arrow).Get<double>("actualDepth")))
            .OrderBy(entry => entry.Depth)
            .ToList();

        if (drilled.Count == 0)
        {
            return [];
        }

        return StateBuffer.Save(writer =>
        {
            writer.Write(drilled.Count);

            foreach (var (arrow, depth) in drilled)
            {
                writer.Write(depth);
                writer.Write(arrow.HasDrilled);
                writer.Write(arrow.NaivePush);
            }
        });
    }

    public static void OnLoadState(byte[] state)
    {
        var saved = new Dictionary<double, (bool HasDrilled, bool NaivePush)>();

        StateBuffer.Load(state, reader =>
        {
            var drilledCount = reader.ReadInt32();

            for (int i = 0; i < drilledCount; i++)
            {
                saved[reader.ReadDouble()] = (reader.ReadBoolean(), reader.ReadBoolean());
            }
        });

        if (saved.Count == 0)
        {
            return;
        }

        var arrows = TfStateInterop.CurrentLevel?[GameTags.Arrow];
        if (arrows is null)
        {
            return;
        }

        foreach (var arrow in arrows.OfType<Arrow>())
        {
            var data = DynamicData.For(arrow);

            if (!saved.TryGetValue(data.Get<double>("actualDepth"), out var toLoad))
            {
                continue;
            }

            data.Set("HasDrilled", toLoad.HasDrilled);
            arrow.NaivePush = toLoad.NaivePush;
        }
    }
}
