using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Lockstep.Protocol;
using Lockstep.Server;
using Lockstep.Util;
using Microsoft.Extensions.Logging;

namespace Lockstep.Net;

internal class PacketAckHandler : IPacketHandler
{
    private readonly ServerContext _context;

    /// <summary>
    /// Requires at least one UInt16 for the sequence number of the acknowledged packet
    /// </summary>
    public uint MinPayloadSize => sizeof(ushort);

    public PacketAckHandler(ServerContext context)
    {
        _context = context;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private readonly ref struct PacketAckPayload
    {
        private readonly ReadOnlySpan<byte> _buffer;

        /// <summary>
        /// The UInt16 sequence number of the ack packet, starting at offset 0x0
        /// </summary>
        public ushort SequenceNumber => BinaryPrimitives.ReadUInt16LittleEndian(_buffer);

        public PacketAckPayload(ReadOnlySpan<byte> buffer)
        {
            _buffer = buffer;
        }
    }

    public void Handle(ParsedPacket packet, Room room)
    {
        PacketAckPayload payload = new PacketAckPayload(packet.Payload);

        ushort sequenceNumber = payload.SequenceNumber;

        PacketPending? pendingPacket = room.Broadcaster.AckPacketStore.RemovePacket(sequenceNumber);
        if (pendingPacket == null)
        {
            _context.Logger.LogError("The packet #{SequenceNumber} was not found in room #{RoomId}", sequenceNumber, room.Id);
            Result<Error>.Failure(Error.OperationFailed);
            return;
        }

        _context.Logger.LogTrace("Successfully Acked packet #{PacketNumber} from {PlayerName} in Room #{RoomId}", pendingPacket.SequenceNumber, pendingPacket.Player.Name, room.Id);
    }
}
