using Lockstep.Protocol;

namespace Lockstep.Client;

internal class Player
{
    public required PlayerInfo Info { get; set; }
    public byte PortNumber { get; init; }
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
    public Dictionary<ushort, PendingPacket> PendingPackets = [];
}
