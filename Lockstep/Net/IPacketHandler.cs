using Lockstep.Protocol;
using Lockstep.Util;

namespace Lockstep.Net;

internal interface IPacketHandler
{
    Result<Error> Handle(ReadOnlySpan<byte> payload);
}
