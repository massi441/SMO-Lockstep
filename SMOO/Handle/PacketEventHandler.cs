using Microsoft.Extensions.Logging;
using SMOO.Event;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Util;

namespace SMOO.Handle;

internal class PacketEventHandler : IPacketHandler
{
    public static ushort MinPayloadSize => sizeof(ushort);

    private ref struct PacketEventPayload : IDeserializableStruct
    {
        public EventType EventType;

        public void Deserialize(ReadOnlySpan<byte> source)
        {
            SpanReader reader = new SpanReader(source);
            EventType = (EventType)reader.ReadUInt16LittleEndian();
        }
    }

    public static void Handle(ParsedPacket packet, Room room, ServerContext context)
    {
        PacketEventPayload payload = PacketSerializer.Deserialize<PacketEventPayload>(packet.Payload);

        context.Logger.LogTrace("Dispatching event {EventType} from {PlayerName}", payload.EventType, packet.SenderPlayer?.Name);

        Event.EventHandler handler = EventHandlerProvider.GetHandler(payload.EventType);

        ReadOnlySpan<byte> eventData = packet.Payload[sizeof(ushort)..];

        if (eventData.Length < handler.MinPayloadSize)
        {
            context.Logger.LogWarning("Event {EventType} payload too small ({Size}), minimum required: {Minimum}", payload.EventType, eventData.Length, handler.MinPayloadSize);
            packet.RentedBuffer.Return();
            return;
        }

        unsafe
        {
            handler.Handle(packet, room, context, eventData); // handler takes ownership of the rented buffer
        }
    }
}
