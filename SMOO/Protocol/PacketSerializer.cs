using SMOO.Util;

namespace SMOO.Protocol;

internal static class PacketSerializer
{
    public static RentedBuffer Serialize<T>(ref T packet, int requiredSize) where T : struct, ISerializableStruct, allows ref struct
    {
        RentedBuffer buffer = new RentedBuffer(requiredSize);
        SpanWriter writer = new SpanWriter(buffer);

        packet.Serialize(ref writer);
        buffer.Restrict(writer.Offset);

        return buffer;
    }

    public static T Deserialize<T>(ReadOnlySpan<byte> source) where T : struct, IDeserializableStruct, allows ref struct
    {
        T t = new T();
        SpanReader reader = new SpanReader(source);
        t.Deserialize(ref reader);
        return t;
    }
}
