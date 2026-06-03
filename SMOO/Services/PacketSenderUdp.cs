using System.Net;
using System.Net.Sockets;
using SMOO.Protocol;
using SMOO.Util;

namespace SMOO.Services;

internal class PacketSenderUdp : IPacketSender
{
    private readonly Socket _socket;

    public PacketSenderUdp(Socket socket)
    {
        _socket = socket;
    }

    public Result<Error> Send(EndPoint destination, ReadOnlySpan<byte> data)
    {
        int bytesSent = _socket.SendTo(data, destination);
        if (bytesSent != data.Length)
        {
            return Result<Error>.Failure(Error.NotSent);
        }

        return Result<Error>.Success();
    }
}
