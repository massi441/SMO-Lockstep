using SMOO.Util;

namespace SMOO.Protocol;

internal static class PacketSerializer
{
    /// <summary>
    /// Serializes a packet into a specified buffer and returns the bytes written
    /// </summary>
    /// <typeparam name="T">The type of packet to serialize</typeparam>
    /// <param name="destination">The destination buffer</param>
    /// <param name="packet">The packet to serialize</param>
    /// <returns></returns>
    public static int Serialize<T>(Span<byte> destination, ref T packet) where T : struct, ISerializableStruct, allows ref struct
    {
        SpanWriter writer = new SpanWriter(destination);

        packet.Serialize(ref writer);

        return writer.Offset;
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
