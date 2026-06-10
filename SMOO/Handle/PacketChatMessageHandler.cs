using System.Text;
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
    public static ushort MinPayloadSize => sizeof(ushort);

    private struct PacketChatMessageRequest : IDeserializableStruct
    {
        public ushort MessageLength;
        public string Message;

        public void Deserialize(ReadOnlySpan<byte> source)
        {
            SpanReader reader = new SpanReader(source);
            MessageLength = reader.ReadUInt16LittleEndian();
            Message = Encoding.UTF8.GetString(reader.ReadBytes(MessageLength));
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
            MessageLength = request.MessageLength,
            Message = request.Message,
        };

        RentedBuffer chatBuffer = new RentedBuffer(chatPacket.Size());
        chatPacket.Serialize(chatBuffer.UsedSpan);

        packet.RentedBuffer.Return();

        room.Broadcaster.BroadcastReliablyExcept(room, packet.SenderPlayer, chatBuffer);
    }
}
