using System.Net;
using System.Runtime.InteropServices;
using Lockstep.Util;

namespace Lockstep.Protocol;

/// <summary>
/// Represents a network packet containing a fully parsed header, and a payload ready for processing by a packet handler
/// </summary>
internal readonly struct ParsedPacket
{
    public required IPEndPoint Sender { get; init; }
    public required RentedBuffer<byte> RentedBuffer { get; init; }

    public PacketHeader Header => MemoryMarshal.Read<PacketHeader>(RentedBuffer.Ref.AsSpan());
    public ReadOnlySpan<byte> Payload => RentedBuffer.Ref.AsSpan(PacketHeader.SizeOf(), Header.PayloadSize);
}
