using Microsoft.Xna.Framework;
using MonoMod.Utils;
using System;
using System.IO;

namespace Teuria.AdditionalVariants;

//Simple State serializer using a BinaryWritter
internal static class StateBuffer
{
    public static byte[] Save(Action<BinaryWriter> write)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        write(writer);
        writer.Flush();
        return stream.ToArray();
    }

    public static void Load(byte[]? state, Action<BinaryReader> read)
    {
        if (state is null || state.Length == 0)
        {
            return;
        }

        using var stream = new MemoryStream(state);
        using var reader = new BinaryReader(stream);
        read(reader);
    }

    public static void WriteVector2(this BinaryWriter writer, Vector2 value)
    {
        writer.Write(value.X);
        writer.Write(value.Y);
    }

    public static Vector2 ReadVector2(this BinaryReader reader)
    {
        return new Vector2(reader.ReadSingle(), reader.ReadSingle());
    }

    public static bool TryGetValue<T>(this DynamicData data, string name, out T value)
    {
        if (data.TryGet(name, out var boxed) && boxed is T typed)
        {
            value = typed;
            return true;
        }

        value = default!;
        return false;
    }
}
