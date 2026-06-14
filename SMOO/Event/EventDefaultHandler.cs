using Microsoft.Extensions.Logging;
using SMOO.Protocol;
using SMOO.Server;

namespace SMOO.Event;

internal class EventDefaultHandler : IEventHandler
{
    public static ushort MinPayloadSize => 0;

    public static void Handle(ParsedEventPacket packet, Room room, ServerContext context)
    {
        context.Logger.LogTrace("Default Event Handler invoked for unhandled event in packet from {PlayerName}", packet.BasePacket.SenderPlayer?.Name);

        packet.BasePacket.RentedBuffer.Return();
    }
}
