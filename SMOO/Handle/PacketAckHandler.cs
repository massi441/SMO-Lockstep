using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SMOO.Protocol;
using SMOO.Server;
using Microsoft.Extensions.Logging;
using SMOO.Client;

namespace SMOO.Handle;

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
    private ref struct PacketAckPayload : IDeserializableStruct
    {
        /// <summary>
        /// The UInt16 sequence number of the ack packet, starting at offset 0x0
        /// </summary>
        public ushort SequenceNumber;

        public void Deserialize(ReadOnlySpan<byte> source)
        {
            SequenceNumber = BinaryPrimitives.ReadUInt16LittleEndian(source);
        }
    }

    public void Handle(Packet packet, Room room, Player? player)
    {
        PacketAckPayload payload = PacketSerializer.Deserialize<PacketAckPayload>(packet.Payload);

        ReliablePacket? pendingPacket = room.Broadcaster.ReliablePacketStore.RemovePacket(payload.SequenceNumber);
        if (pendingPacket == null)
        {
            _context.Logger.LogError("The packet #{SequenceNumber} was not found in room #{RoomId}", payload.SequenceNumber, room.Id);
            return;
        }

        _context.Logger.LogTrace("Successfully Acked {PacketType} packet #{PacketNumber} from {PlayerName} in Room #{RoomId}", packet.Header.Type, pendingPacket.SequenceNumber, pendingPacket.Player.Name, room.Id);
    }
}
