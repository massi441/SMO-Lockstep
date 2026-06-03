using System.Net;
using SMOO.Protocol;
using SMOO.Util;

namespace SMOO.Services;

internal interface IPacketSender
{
    Result<Error> Send(EndPoint destination, ReadOnlySpan<byte> data);
}
