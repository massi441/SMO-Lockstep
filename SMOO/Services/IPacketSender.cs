using System.Net;
using Lockstep.Protocol;
using Lockstep.Util;

namespace Lockstep.Services;

internal interface IPacketSender
{
    Result<Error> Send(EndPoint destination, ReadOnlySpan<byte> data);
}
