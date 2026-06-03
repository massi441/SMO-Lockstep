using System.Net;
using System.Runtime.InteropServices;
using Lockstep.Util;

namespace Lockstep.Protocol;

/// <summary>
/// Represents a network packet containing a fully parsed header, and a payload ready for processing by a packet handler
/// </summary>
internal readonly struct ParsedPacket
{
    /// <summary>
    /// The sender of the packet
    /// </summary>
    public required IPEndPoint Sender { get; init; }

    /// <summary>
    /// The rented buffer of the packet
    /// </summary>
    public required RentedBuffer<byte> RentedBuffer { get; init; }

    /// <summary>
    /// Returns a view of the header inside the packet's payload
    /// </summary>
    public ref PacketHeader Header => ref MemoryMarshal.AsRef<PacketHeader>(RentedBuffer.Ref.AsSpan());

    /// <summary>
    /// Returns a span of the payload of the packet
    /// </summary>
    public ReadOnlySpan<byte> Payload => RentedBuffer.Ref.AsSpan(PacketHeader.SizeOf(), Header.PayloadSize);
}
