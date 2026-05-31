using System.Collections.Concurrent;
using Lockstep.Protocol;
using Lockstep.Util;

namespace Lockstep.Net;

internal class PendingPacketStore : IPendingPacketStore
{
    private ushort _nextSequenceNumber = 0;
    private readonly ConcurrentDictionary<ushort, PendingPacket> _pendingPackets = [];
    public ConcurrentDictionary<ushort, PendingPacket> PendingPackets => _pendingPackets;

    public Result<Error> UploadPacket(in PendingPacketRequest request)
    {
        if (IsFull())
        {
            return Result<Error>.Failure(Error.PendingPacketStoreFull);
        }

        PendingPacket pendingPacket = new PendingPacket(request.MaxRetries)
        {
            Receiver = request.Receiver,
            Payload = request.Payload,
            SequenceNumber = _nextSequenceNumber
        };

        _pendingPackets[_nextSequenceNumber] = pendingPacket;
        _nextSequenceNumber++;

        return Result<Error>.Success();
    }

    public Result<Error> RemovePacket(ushort sequenceNumber)
    {
        if (_pendingPackets.TryRemove(sequenceNumber, out PendingPacket? _))
        {
            return Result<Error>.Success();
        }

        return Result<Error>.Failure(Error.PendingPacketNotFound);
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
