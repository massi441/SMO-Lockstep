using System.Net;
using Lockstep.Protocol;
using Lockstep.Util;

namespace Lockstep.Net;

internal interface IPacketSender
{
    Result<Error> Send(EndPoint destination, ReadOnlySpan<byte> data);
}
