namespace Lockstep.Protocol;

internal class Packet
{
    public PacketHeader Header { get; init; }
    public Payload Payload { get; init; }
}
