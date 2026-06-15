using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SMOO.Client;
using SMOO.Util;

namespace SMOO.Protocol;

/// <summary>
/// Represents a network packet with a parsed header, ready to be handled by a server handler
/// </summary>
internal readonly struct ParsedPacket
{
    public required IPEndPoint SenderIp { get; init; }
    public required RentedBuffer RentedBuffer { get; init; }
    public Player? SenderPlayer { get; init; }

    /// <summary>
    /// Returns a view of the header inside the packet's payload
    /// </summary>
    public ref PacketHeader Header => ref MemoryMarshal.AsRef<PacketHeader>(RentedBuffer.RentRef.AsSpan());

    /// <summary>
    /// Returns a span of the payload of the packet
    /// </summary>
    public Span<byte> Payload => RentedBuffer.UsedSpan[Unsafe.SizeOf<PacketHeader>()..];

    public int FullSize => RentedBuffer.UsedBytes;
}
