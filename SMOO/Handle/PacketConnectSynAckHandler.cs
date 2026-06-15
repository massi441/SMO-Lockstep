using Microsoft.Extensions.Logging;
using SMOO.Client;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Util;

namespace SMOO.Handle;

internal class PacketConnectSynAckHandler : IPacketHandler
{
    public static ushort MinPayloadSize => 0;
    public static ushort MaxPayloadSize => 0;

    public static void Handle(ParsedPacket packet, Room room, ServerContext context)
    {
        ushort sequenceNumber = packet.Header.SequenceNumber;

        ReliablePacket? ackPacket = room.Broadcaster.ReliablePacketStore.RemovePacket(packet.SenderPlayer!, sequenceNumber);
        if (ackPacket == null)
        {
            context.Logger.LogWarning("Invalid SYN ACK sequence number ({SequenceNumber}) received by {PlayerName} in Room #{RoomId}, broadcast will be skipped", sequenceNumber, packet.SenderPlayer?.Name, room.Id);
            packet.RentedBuffer.Return();
            return;
        }

        PacketPlayerJoinRoom joinPacket = new PacketPlayerJoinRoom()
        {
            Header = packet.Header.WithType(PacketType.PlayerJoinRoom),
            PlayerRoomInfo = new PlayerInRoomInfo(packet.SenderPlayer!)
        };

        RentedBuffer joinRoomBuffer = PacketSerializer.Serialize(ref joinPacket, RequiredSize<PacketPlayerJoinRoom>.MaxSize);

        context.Logger.LogInformation("Player {PlayerName} has confirmed their connection in Room #{RoomId}, room will be notified", packet.SenderPlayer!.Name, room.Id);

        room.Broadcaster.BroadcastReliably(room.Players.Except(packet.SenderPlayer), joinRoomBuffer); // transfers ownership of the rented buffer to the reliable store

        packet.RentedBuffer.Return();
    }
}
