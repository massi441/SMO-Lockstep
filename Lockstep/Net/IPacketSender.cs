using System.Net;
using System.Net.Sockets;
using Lockstep.Protocol;
using Lockstep.Util;

namespace Lockstep.Net;

internal interface IPacketSender
{
    Result<Error> Send(ReadOnlySpan<byte> data, EndPoint destination);
}
