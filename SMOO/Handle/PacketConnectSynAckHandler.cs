using System.Buffers.Binary;
using Microsoft.Extensions.Logging;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Util;

namespace SMOO.Handle;

internal class PacketConnectSynAckHandler : IPacketHandler
{
    public static ushort MinPayloadSize => 0;

    /// <summary>
    /// The payload sent by a new player, to confirm that they have joined a room
    /// </summary>
    private ref struct PacketConnectSynAckPayload : IDeserializableStruct
    {
        public ushort SequenceNumber { get; private set; }

        public void Deserialize(ReadOnlySpan<byte> source)
        {
            SequenceNumber = BinaryPrimitives.ReadUInt16LittleEndian(source);
        }
    }

    public static void Handle(ParsedPacket packet, Room room, ServerContext context)
    {
        PacketConnectSynAckPayload synAckPayload = new PacketConnectSynAckPayload();

        synAckPayload.Deserialize(packet.Payload);

        ReliablePacket? ackPacket = room.Broadcaster.ReliablePacketStore.RemovePacket(synAckPayload.SequenceNumber);
        if (ackPacket == null)
        {
            packet.RentedBuffer.Return();
            context.Logger.LogWarning("Invalid SYN ACK sequence number ({SequenceNumber}) received by {PlayerName} in Room #{RoomId}, broadcast will be skipped", synAckPayload.SequenceNumber, packet.SenderPlayer?.Name, room.Id);
            return;
        }

        PacketPlayerJoinRoom joinPacket = new PacketPlayerJoinRoom()
        {
            Header = packet.Header.WithSizeType(MemoryUtil.PayloadSize<PacketPlayerJoinRoom>(), PacketType.PlayerJoinRoom),
            PlayerSlot = packet.SenderPlayer!.Slot,
            PlayerNameLength = (byte)packet.SenderPlayer!.Name.Length,
            PlayerName = packet.SenderPlayer!.Name,
        };

        RentedBuffer joinRoomBuffer = new RentedBuffer(joinPacket.Size());

        PacketSerializer.Serialize(joinRoomBuffer.UsedSpan, in joinPacket);

        context.Logger.LogInformation("Player {PlayerName} has confirmed their connection in Room #{RoomId}, room will be notified", packet.SenderPlayer.Name, room.Id);

        room.Broadcaster.BroadcastReliablyExcept(room, packet.SenderPlayer, joinRoomBuffer); // transfers ownership of the rented buffer to the reliable store
    }
}
