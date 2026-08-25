using TowerFall;

namespace Teuria.AdditionalVariants;

internal static class AnnoyingMageState
{
    public const string Name = "AnnoyingMage";

    public static byte[] OnSaveState()
    {
        var sequence = FindSequence(TfStateInterop.CurrentLevel);
        if (sequence is null)
        {
            return [];
        }

        return StateBuffer.Save(writer =>
        {
            writer.Write(sequence.Phase);
            writer.Write(sequence.Counter);
        });
    }

    public static void OnLoadState(byte[] state)
    {
        var sequence = FindSequence(TfStateInterop.CurrentLevel);
        if (sequence is null)
        {
            return;
        }

        StateBuffer.Load(state, reader =>
        {
            sequence.Phase = reader.ReadInt32();
            sequence.Counter = reader.ReadSingle();
        });
    }

    private static InvincibleTechnomageVariantSequence? FindSequence(Level? level)
    {
        if (level is null)
        {
            return null;
        }

        foreach (var layer in level.Layers.Values)
        {
            foreach (var entity in layer.Entities)
            {
                if (entity is InvincibleTechnomageVariantSequence sequence)
                {
                    return sequence;
                }
            }
        }

        return null;
    }
}
