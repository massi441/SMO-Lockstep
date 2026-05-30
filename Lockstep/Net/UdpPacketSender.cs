using System.Net;
using System.Net.Sockets;
using Lockstep.Protocol;
using Lockstep.Util;

namespace Lockstep.Net;

internal class UdpPacketSender : IPacketSender
{
    private readonly Socket _socket;

    public UdpPacketSender(Socket socket)
    {
        _socket = socket;
    }

    public Result<Error> Send(ReadOnlySpan<byte> data, EndPoint destination)
    {
        int bytesSent = _socket.SendTo(data, destination);
        if (bytesSent != data.Length)
        {
            return Result<Error>.Failure(Error.NotSent);
        }

        return Result<Error>.Success();
    }
}
