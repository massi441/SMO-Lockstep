using System.Buffers;
using System.Collections.Concurrent;
using SMOO.Protocol;
using SMOO.Services.Interface;
using SMOO.Util;

namespace SMOO.Services.Impl;

internal class PendingPacketStore : IPendingPacketStore
{
    private ushort _nextSequenceNumber = 0;
    private readonly ConcurrentDictionary<ushort, PendingPacket> _pendingPackets = [];

    public ConcurrentDictionary<ushort, PendingPacket> PendingPackets => _pendingPackets;

    public Result<Error> UploadPacket(PendingPacketRequest request)
    {
        if (IsFull())
        {
            return Result<Error>.Failure(Error.PendingPacketStoreFull);
        }

        PendingPacket pendingPacket = new PendingPacket()
        {
            Player = request.Receiver,
            RentedPayload = request.RentedPayload,
            Tries = request.MaxRetries,
            SequenceNumber = _nextSequenceNumber
        };

        WriteSequence(pendingPacket.RentedPayload.Ref, pendingPacket.SequenceNumber);

        _pendingPackets[_nextSequenceNumber] = pendingPacket;
        _nextSequenceNumber++;

        return Result<Error>.Success();
    }

    public PendingPacket? RemovePacket(ushort sequenceNumber)
    {
        if (_pendingPackets.TryRemove(sequenceNumber, out PendingPacket? pendingPacket))
        {
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
