using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using SMOO.Client;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Services.Interface;
using SMOO.Util;

namespace SMOO.Services.Impl;

internal class ReliablePacketStore : IReliablePacketStore
{
    private ushort _nextSequenceNumber = 0;
    private readonly ServerContext _context;
    private readonly ConcurrentDictionary<ushort, ReliablePacket> _pendingPackets = [];

    public ConcurrentDictionary<ushort, ReliablePacket> PendingPackets => _pendingPackets;

    public ReliablePacketStore(ServerContext context)
    {
        _context = context;
    }

    public Result<Error> UploadPacket(RentedBuffer rentedBuffer, AtomicCounter refCounter, Player receiver, byte maxRetries)
    {
        if (IsFull())
        {
            return Result<Error>.Failure(Error.PendingPacketStoreFull);
        }

        ReliablePacket reliablePacket = new ReliablePacket()
        {
            RentedBuffer = rentedBuffer,
            RefCounter = refCounter,
            Receiver = receiver,
            Tries = maxRetries,
            SequenceNumber = _nextSequenceNumber
        };

        reliablePacket.WriteSequenceNumber();
        reliablePacket.RefCounter.Increment();

        _pendingPackets[_nextSequenceNumber] = reliablePacket;
        _nextSequenceNumber++;

        _context.Logger.LogTrace("Uploaded reliable {PacketType} packet with sequence number #{SequenceNumber}, and {Tries} tries", reliablePacket.Header.Type, reliablePacket.SequenceNumber, reliablePacket.Tries);

        return Result<Error>.Success();
    }

    public ReliablePacket? RemovePacket(ushort sequenceNumber)
    {
        if (_pendingPackets.TryRemove(sequenceNumber, out ReliablePacket? pendingPacket))
        {
            if (pendingPacket.RefCounter.Decrement() == 0)
            {
                pendingPacket.RentedBuffer.Return();
                _context.Logger.LogTrace("Removed and free'd buffer used by reliable packet #{SequenceNumber}", sequenceNumber);
            }
            else
            {
                _context.Logger.LogTrace("Decremented ref count after removing reliable packet #{SequenceNumber}, new ref count: {RefCount}", sequenceNumber, pendingPacket.RefCounter.Count);
            }
            
            return pendingPacket;
        }

        return null;
    }

    private bool IsFull()
    {
        return _pendingPackets.Count > ushort.MaxValue;
    }
}
