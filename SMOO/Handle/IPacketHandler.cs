using SMOO.Protocol;
using SMOO.Server;

namespace SMOO.Handle;

internal interface IPacketHandler
{
    static abstract ushort MinPayloadSize { get; }
    static abstract void Handle(ParsedPacket packet, Room room, ServerContext context);
}
