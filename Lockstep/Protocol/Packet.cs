using System.Net;

namespace Lockstep.Protocol;

/// <summary>
/// Represents a network packet containing a fully parsed header,
/// and a payload ready for processing by a packet handler
/// </summary>
internal class Packet
{
    public PacketHeader Header { get; init; }
    public Payload Payload { get; init; }

    public IPEndPoint Sender => Payload.Sender;
}
