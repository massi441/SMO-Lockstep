using System.Buffers.Binary;
using SMOO.Protocol;
using SMOO.Server;
using Microsoft.Extensions.Logging;
using SMOO.Util;

namespace SMOO.Handle;

internal static class PacketAckHandler
{

    /// <summary>
    /// Requires at least one UInt16 for the sequence number of the acknowledged packet
    /// </summary>
    public static ushort MinPayloadSize => sizeof(ushort);

    private ref struct PacketAckPayload : IDeserializableStruct
    {
        public ushort SequenceNumber;

        public void Deserialize(ReadOnlySpan<byte> source)
        {
            SequenceNumber = BinaryPrimitives.ReadUInt16LittleEndian(source);
        }
    }

    public static void Handle(ParsedPacket packet, Room room, ServerContext context)
    {
        PacketAckPayload payload = PacketSerializer.Deserialize<PacketAckPayload>(packet.Payload);

        ReliablePacket? pendingPacket = room.Broadcaster.ReliablePacketStore.RemovePacket(payload.SequenceNumber);
        if (pendingPacket == null)
        {
            packet.RentedBuffer.Return();
            context.Logger.LogError("The packet #{SequenceNumber} was not found in room #{RoomId}", payload.SequenceNumber, room.Id);
            return;
        }

        packet.RentedBuffer.Return();

        context.Logger.LogTrace("Successfully Acked {PacketType} packet #{PacketNumber} from {PlayerName} in Room #{RoomId}", packet.Header.Type, pendingPacket.SequenceNumber, pendingPacket.Receiver.Name, room.Id);
    }
}
