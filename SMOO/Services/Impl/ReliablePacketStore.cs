using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
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

    public Result<Error> UploadPacket(ReliablePacketRequest request)
    {
        if (IsFull())
        {
            return Result<Error>.Failure(Error.PendingPacketStoreFull);
        }

        ReliablePacket pendingPacket = new ReliablePacket()
        {
            Player = request.Receiver,
            RentedPayload = request.RentedPayload,
            Tries = request.MaxRetries,
            SequenceNumber = _nextSequenceNumber
        };

        WriteSequence(pendingPacket.RentedPayload.Ref, pendingPacket.SequenceNumber);

        _pendingPackets[_nextSequenceNumber] = pendingPacket;
        _nextSequenceNumber++;

        _context.Logger.LogTrace("Uploaded reliable packet with sequence number #{SequenceNumber}, and {Tries} tries", pendingPacket.SequenceNumber, pendingPacket.Tries);

        return Result<Error>.Success();
    }

    public ReliablePacket? RemovePacket(ushort sequenceNumber)
    {
        if (_pendingPackets.TryRemove(sequenceNumber, out ReliablePacket? pendingPacket))
        {
            _context.Logger.LogTrace("Removing and freeing buffer used by reliable packet #{SequenceNumber}", sequenceNumber);
            pendingPacket.RentedPayload.Return();
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

    private static void WriteSequence(byte[] payload, ushort sequenceNumber)
    {
        SpanWriter writer = new SpanWriter(payload);

        writer.Jump(PacketHeader.SizeOf());

        writer.Write(sequenceNumber);
    }
}
