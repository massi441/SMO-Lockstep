using System.Collections.Concurrent;
using Lockstep.Protocol;
using Lockstep.Util;

namespace Lockstep.Net;

internal class PacketPendingStore : IPacketPendingStore
{
    private ushort _nextSequenceNumber = 0;
    private readonly ConcurrentDictionary<ushort, PacketPending> _pendingPackets = [];

    public ConcurrentDictionary<ushort, PacketPending> PendingPackets => _pendingPackets;

    public Result<Error> UploadPacket(in PacketPendingRequest request)
    {
        if (IsFull())
        {
            return Result<Error>.Failure(Error.PendingPacketStoreFull);
        }

        PacketPending pendingPacket = new PacketPending()
        {
            Player = request.Receiver,
            Payload = request.Payload,
            Tries = request.MaxRetries,
            SequenceNumber = _nextSequenceNumber
        };

        _pendingPackets[_nextSequenceNumber] = pendingPacket;
        _nextSequenceNumber++;

        return Result<Error>.Success();
    }

    public PacketPending? RemovePacket(ushort sequenceNumber)
    {
        if (_pendingPackets.TryRemove(sequenceNumber, out PacketPending? pendingPacket))
        {
            return pendingPacket;
        }

        return null;
    }

    private bool IsFull()
    {
        if (_nextSequenceNumber > 0)
        {
            return false;
        }

        for (ushort i = 0; i < ushort.MaxValue; i++)
        {
            if (!_pendingPackets.ContainsKey(i))
            {
                return false;
            }
        }

        return true;
    }
}
