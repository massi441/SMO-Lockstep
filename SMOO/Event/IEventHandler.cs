using SMOO.Protocol;
using SMOO.Server;

namespace SMOO.Event;

internal interface IEventHandler
{
    static abstract ushort MinDataSize { get; }
    static abstract ushort MaxDataSize { get; }
    static abstract void Handle(ParsedEventPacket packet, Room room, ServerContext context);
}
