using SMOO.Protocol;
using SMOO.Server;

namespace SMOO.Event;

internal interface IEventHandler
{
    static abstract ushort MinPayloadSize { get; }
    static abstract void Handle(ParsedEventPacket packet, Room room, ServerContext context);
}
