using System.Net.Sockets;
using Lockstep.Protocol;
using Lockstep.Util;

namespace Lockstep.Net;

internal interface IPacketHandler
{
    Result<Error> Handle(Payload packetPayload);
}
