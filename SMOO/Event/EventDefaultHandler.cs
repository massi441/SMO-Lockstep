using Microsoft.Extensions.Logging;
using SMOO.Protocol;
using SMOO.Server;

namespace SMOO.Event;

internal class EventDefaultHandler : IEventHandler
{
    public static ushort MinPayloadSize => 0;

    public static void Handle(ParsedPacket packet, Room room, ServerContext context, ReadOnlySpan<byte> eventData)
    {
        context.Logger.LogTrace("Default Event Handler invoked for unhandled event in packet from {PlayerName}", packet.SenderPlayer?.Name);
    }
}
