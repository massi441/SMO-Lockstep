using SMOO.Handle;
using SMOO.Protocol;
using SMOO.Server;

namespace SMOO.Services.Interface;

internal interface IPacketHandlerProvider
{
    IPacketHandler? GetShared(PacketType packetType, ServerContext context);
    IPacketHandler? GetNew(PacketType packetType, ServerContext context);
}
