using SMOO.Protocol;
using SMOO.Server;

namespace SMOO.Event;

internal interface IEventHandler
{
    static abstract ushort MinPayloadSize { get; }
    static abstract void Handle(ParsedPacket packet, Room room, ServerContext context, ReadOnlySpan<byte> eventData);
}
