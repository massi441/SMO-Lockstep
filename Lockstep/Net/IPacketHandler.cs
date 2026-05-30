using Lockstep.Protocol;
using Lockstep.Server;
using Lockstep.Util;

namespace Lockstep.Net;

internal interface IPacketHandler
{
    Result<Error> Handle(Packet packet, Room room);
}
