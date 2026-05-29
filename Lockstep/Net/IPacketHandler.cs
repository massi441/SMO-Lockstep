using Lockstep.Protocol;
using Lockstep.Server;
using Lockstep.Util;

namespace Lockstep.Net;

internal interface IPacketHandler
{
    Result<Error> Handle(Room room, Payload packetPayload);
}
