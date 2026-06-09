using System.Text;
using Microsoft.Extensions.Logging;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Util;

namespace SMOO.Handle;

// will add proper features later {message length, sender, scope...}
internal static class PacketChatMessageHandler
{
    /// <summary>
    /// Requires the size of the chat message to be provided
    /// </summary>
    public static ushort MinPayloadSize => 2;

    private struct PacketChatMessagePayload : IDeserializableStruct
    {
        public ushort SequenceNumber;
        public ushort MessageLength;
        public string Message;

        public void Deserialize(ReadOnlySpan<byte> source)
        {
            SpanReader reader = new SpanReader(source);

            reader.Skip(sizeof(ushort));

            MessageLength = (ushort)Math.Max(reader.ReadUInt16LittleEndian(), reader.Remaining);
            Message = Encoding.UTF8.GetString(reader.ReadBytes(MessageLength));
        }
    }

    public static void Handle(ParsedPacket packet, Room room, ServerContext context)
    {
        PacketChatMessagePayload payload = PacketSerializer.Deserialize<PacketChatMessagePayload>(packet.Payload);

        context.Logger.LogTrace("{PlayerName} sent a message in room #{RoomId}: {Message}", packet.SenderPlayer!.Name, room.Id, payload.Message);

        // TODO: Copy message into new buffer post sanitization

        room.Broadcaster.BroadcastReliablyExcept(room, packet.SenderPlayer, packet.RentedBuffer, Config.MaxRetries); // transfers ownership of the buffer to the reliable packet store
    }
}
