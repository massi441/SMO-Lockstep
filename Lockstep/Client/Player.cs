using System.Collections.Concurrent;
using Lockstep.Protocol;

namespace Lockstep.Client;

internal class Player
{
    private ushort _nextSequenceNumber = 0;
    public required PlayerInfo Info { get; set; }
    public byte PortNumber { get; init; }
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
    public ConcurrentDictionary<ushort, PacketPending> PendingPackets = [];

    public ushort NextSequenceNumber => _nextSequenceNumber++;
}
