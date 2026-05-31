using System.Buffers;
using Lockstep.Protocol;
using Lockstep.Server;
using Lockstep.Util;
using Microsoft.Extensions.Logging;

namespace Lockstep.Net;

internal class PacketAckHandler : IPacketHandler
{
    private readonly ServerContext _context;

    public PacketAckHandler(ServerContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Requires at least one UInt16 for the sequence number of the acknowledged packet
    /// </summary>
    public uint MinPayloadSize => sizeof(ushort);

    public Result<Error> Handle(Packet packet, Room room)
    {
        SpanReader reader = new SpanReader(packet.Payload.Buffer);

        ushort sequenceNumber = reader.ReadUInt16LittleEndian();

        PacketPending? pendingPacket = room.Broadcaster.AckPacketStore.RemovePacket(sequenceNumber);
        if (pendingPacket == null)
        {
            _context.Logger.LogError("An error occured while trying to remove packet #{SequenceNumber} in room #{RoomId}", sequenceNumber, room.Id);
            return Result<Error>.Failure(Error.OperationFailed);
        }

        ArrayPool<byte>.Shared.Return(pendingPacket.Payload);

        _context.Logger.LogTrace("Successfully Acked packet #{PacketNumber} in Room #{RoomId}", pendingPacket.SequenceNumber, room.Id);

        return Result<Error>.Success();
    }
}
