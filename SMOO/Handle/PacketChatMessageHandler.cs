using Microsoft.Extensions.Logging;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Util;

namespace SMOO.Handle;

internal class PacketChatMessageHandler : IPacketHandler
{
    /// <summary>
    /// Requires one UInt16 for the length of the message
    /// </summary>
    public static ushort MinPayloadSize => RequiredSize<PacketChatMessageRequest>.MinSize;
    public static ushort MaxPayloadSize => RequiredSize<PacketChatMessageRequest>.MaxSize;

    private struct PacketChatMessageRequest : IDeserializableStruct
    {
        [DynamicField(MaxSize = Config.MaxChatMessageLength)]
        public StreamStringView<ushort> Message;

        public void Deserialize(ref SpanReader reader)
        {
            Message.Deserialize(ref reader, Config.MaxChatMessageLength);
        }
    }

    public static void Handle(ParsedPacket packet, Room room, ServerContext context)
    {
        PacketChatMessageRequest request = PacketSerializer.Deserialize<PacketChatMessageRequest>(packet.Payload);

        context.Logger.LogTrace("{PlayerName} sent a message in room #{RoomId}: {Message}", packet.SenderPlayer!.Name, room.Id, request.Message);

        PacketChatMessage chatPacket = new PacketChatMessage()
        {
            Header = packet.Header.WithType(PacketType.ChatMessage),
            PlayerSlot = packet.SenderPlayer!.Slot,
            Message = request.Message,
        };

        packet.RentedBuffer.Return();

        RentedBuffer chatBuffer = PacketSerializer.Serialize(ref chatPacket, RequiredSize<PacketChatMessage>.MaxSize);

        room.Broadcaster.BroadcastReliably(room.PlayerHolder.PlayersExcept(packet.SenderPlayer), chatBuffer);
    }
}
