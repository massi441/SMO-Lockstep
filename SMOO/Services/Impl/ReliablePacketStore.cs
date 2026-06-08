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

    public Result<Error> UploadPacket(RentedBuffer rentedBuffer, Player receiver, byte maxRetries)
    {
        if (IsFull())
        {
            return Result<Error>.Failure(Error.PendingPacketStoreFull);
        }

        ReliablePacket reliablePacket = new ReliablePacket()
        {
            RentedBuffer = rentedBuffer,
            Receiver = receiver,
            Tries = maxRetries,
            SequenceNumber = _nextSequenceNumber
        };

        WriteSequence(reliablePacket.RentedBuffer.RentRef, reliablePacket.SequenceNumber);

        _pendingPackets[_nextSequenceNumber] = reliablePacket;
        _nextSequenceNumber++;

        _context.Logger.LogTrace("Uploaded reliable packet with sequence number #{SequenceNumber}, and {Tries} tries", reliablePacket.SequenceNumber, reliablePacket.Tries);

        return Result<Error>.Success();
    }

    public ReliablePacket? RemovePacket(ushort sequenceNumber)
    {
        if (_pendingPackets.TryRemove(sequenceNumber, out ReliablePacket? pendingPacket))
        {
            pendingPacket.RentedBuffer.Return();
            _context.Logger.LogTrace("Removed and free'd buffer used by reliable packet #{SequenceNumber}", sequenceNumber);
            return pendingPacket;
        }

        return null;
    }

    private bool IsFull()
    {
        return _pendingPackets.Count > ushort.MaxValue;
    }

    private static void WriteSequence(byte[] payload, ushort sequenceNumber)
    {
        SpanWriter writer = new SpanWriter(payload);

        writer.Jump(PacketHeader.SizeOf());

        writer.Write(sequenceNumber);
    }
}
