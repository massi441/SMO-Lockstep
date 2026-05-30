namespace Lockstep.Protocol;

/// <summary>
/// Represents a network packet containing a fully parsed header,
/// and a payload ready for processing by the server
/// </summary>
internal class Packet
{
    public PacketHeader Header { get; init; }
    public Payload Payload { get; init; }
}
