using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SMOO.Protocol;
using SMOO.Util;

namespace SMOO.Server;

internal static class PacketUtil
{
    public static void WriteSequenceNumber(Span<byte> destination, ushort sequenceNumber)
    {
        SpanWriter writer = new SpanWriter(destination);

        int sequenceOffset = (int)Marshal.OffsetOf<PacketHeader>(nameof(PacketHeader.SequenceNumber));

        writer.Skip(sequenceOffset);
        writer.Write(sequenceNumber);
    }
}
