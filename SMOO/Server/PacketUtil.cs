using System.Runtime.InteropServices;
using SMOO.Protocol;
using SMOO.Util;

namespace SMOO.Server;

internal static class PacketUtil
{
    public static void WritePayloadSize(Span<byte> destination, ushort payloadSize)
    {
        SpanWriter writer = new SpanWriter(destination);

        int sizeOffset = (int)Marshal.OffsetOf<PacketHeader>(nameof(PacketHeader.PayloadSize));

        writer.Skip(sizeOffset);
        writer.Write(payloadSize);
    }

    public static void WriteSequenceNumber(Span<byte> destination, ushort sequenceNumber)
    {
        SpanWriter writer = new SpanWriter(destination);

        int sequenceOffset = (int)Marshal.OffsetOf<PacketHeader>(nameof(PacketHeader.SequenceNumber));

        writer.Skip(sequenceOffset);
        writer.Write(sequenceNumber);
    }
}
