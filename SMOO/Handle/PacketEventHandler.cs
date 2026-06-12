using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using SMOO.Event;
using SMOO.Protocol;
using SMOO.Server;

namespace SMOO.Handle;

internal class PacketEventHandler : IPacketHandler
{
    public static ushort MinPayloadSize => (ushort)Unsafe.SizeOf<EventHeader>();

    public static void Handle(ParsedPacket packet, Room room, ServerContext context)
    {
        ParsedEventPacket eventPacket = new ParsedEventPacket() { BasePacket = packet };

        EventType eventType = eventPacket.EventHeader.Type;

        context.Logger.LogTrace("Dispatching event {EventType} from {PlayerName}", eventType, packet.SenderPlayer?.Name);

        Event.EventHandler handler = EventHandlerTable.GetHandler(eventType);

        if (eventPacket.EventData.Length < handler.MinPayloadSize)
        {
            context.Logger.LogWarning("Event {EventType} payload too small ({Size}), minimum required: {Minimum}", eventType, eventPacket.EventData.Length, handler.MinPayloadSize);
            packet.RentedBuffer.Return();
            return;
        }

        unsafe
        {
            eventPacket.EventHeader.PlayerSlot = packet.SenderPlayer!.Slot;
            handler.Handle(eventPacket, room, context); // handler takes ownership of the rented buffer
        }
    }
}
