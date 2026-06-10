using System.Text;
using Microsoft.Extensions.Logging;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Util;

namespace SMOO.Handle;

internal static class PacketChatMessageHandler
{
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

    private struct PacketChatMessage : ISerializableStruct
    {
        public required PacketHeader Header;
        public ushort SequenceNumber;
        public required byte PlayerSlot;
        public required ushort MessageLength;
        public required string Message;

        public readonly int Size()
        {
            SizeStream stream = new SizeStream();
            stream.Write<PacketHeader>();
            stream.Write<ushort>();
            stream.Write<byte>();
            stream.Write<ushort>();
            stream.WriteString(Message);
            return stream.Size;
        }

        public readonly void Serialize(Span<byte> destination)
        {
            SpanWriter writer = new SpanWriter(destination);
            writer.Write(Header);
            writer.Skip(sizeof(ushort)); // sequence number written at send time
            writer.Write(PlayerSlot);
            writer.Write(MessageLength);
            writer.WriteString(Message);
        }
    }

    // TODO: Copy message into new buffer post sanitization
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
