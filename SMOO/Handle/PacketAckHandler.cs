using System.Buffers.Binary;
using SMOO.Protocol;
using SMOO.Server;
using Microsoft.Extensions.Logging;
using SMOO.Util;

namespace SMOO.Handle;

internal class PacketAckHandler : IPacketHandler
{
    public static ushort MinPayloadSize => 0;

    public static void Handle(ParsedPacket packet, Room room, ServerContext context)
    {
        ushort sequenceNumber = packet.Header.SequenceNumber;

        ReliablePacket? pendingPacket = room.Broadcaster.ReliablePacketStore.RemovePacket(sequenceNumber);
        if (pendingPacket == null)
        {
            packet.RentedBuffer.Return();
            context.Logger.LogError("The packet #{SequenceNumber} was not found in room #{RoomId}", sequenceNumber, room.Id);
            return;
        }

        packet.RentedBuffer.Return();

        context.Logger.LogTrace("Successfully Acked {PacketType} packet #{PacketNumber} from {PlayerName} in Room #{RoomId}", packet.Header.Type, pendingPacket.SequenceNumber, pendingPacket.Receiver.Name, room.Id);
    }
}
