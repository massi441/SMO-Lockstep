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

    public void Handle(Packet packet, Room room)
    {
        SpanReader reader = new SpanReader(packet.Payload.Buffer);

        ushort sequenceNumber = reader.ReadUInt16LittleEndian();

        PacketPending? pendingPacket = room.Broadcaster.AckPacketStore.RemovePacket(sequenceNumber);
        if (pendingPacket == null)
        {
            _context.Logger.LogError("The packet #{SequenceNumber} was not found in room #{RoomId}", sequenceNumber, room.Id);
            Result<Error>.Failure(Error.OperationFailed);
            return;
        }

        ArrayPool<byte>.Shared.Return(pendingPacket.Payload);

        _context.Logger.LogTrace("Successfully Acked packet #{PacketNumber} from {PlayerName} in Room #{RoomId}", pendingPacket.SequenceNumber, pendingPacket.Player.Name, room.Id);
    }
}
