using SMOO.Util;

namespace SMOO.Protocol;

internal static class PacketSerializer
{
    public static void Serialize<T>(Span<byte> destination, in T packet) where T : struct, ISerializableStruct, allows ref struct
    {
        packet.Serialize(destination);
    }

    public static void Deserialize<T>(ReadOnlySpan<byte> source, ref T packet) where T : struct, IDeserializableStruct, allows ref struct
    {
        SpanReader reader = new SpanReader(source);
        packet.Deserialize(ref reader);
    }

    public static T Deserialize<T>(ref SpanReader reader) where T : struct, IDeserializableStruct, allows ref struct
    {
        T t = new T();
        t.Deserialize(ref reader);
        return t;
    }

    public static T Deserialize<T>(ReadOnlySpan<byte> source) where T : struct, IDeserializableStruct, allows ref struct
    {
        T t = new T();
        SpanReader reader = new SpanReader(source);
        t.Deserialize(ref reader);
        return t;
    }
}
