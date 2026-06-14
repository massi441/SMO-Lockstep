using System.Net;
using System.Net.Sockets;
using SMOO.Client;
using SMOO.Protocol;
using SMOO.Services.Interface;
using SMOO.Util;

namespace SMOO.Services.Impl;

internal class PacketSenderUdp : IPacketSender
{
    private readonly Socket _socket;

    public PacketSenderUdp(Socket socket)
    {
        _socket = socket;
    }

    public Result<Error> Send(EndPoint destination, RentedBuffer buffer)
    {
        try
        {
            int bytesSent = _socket.SendTo(buffer, destination);
            if (bytesSent != buffer.UsedBytes)
            {
                return Result<Error>.Failure(Error.NotSent);
            }
        }
        catch (Exception)
        {
            return Result<Error>.Failure(Error.OperationFailed);
        }

        return Result<Error>.Success();
    }

    public Result<Error> SendReliably(Player receiver, RentedBuffer buffer, IReliablePacketStore reliableStore, byte maxRetries = 5)
    {
        RefCounter refCounter = new RefCounter();

        Result<ReliablePacket, Error> uploadResult = reliableStore.UploadPacket(buffer, refCounter, receiver, maxRetries);
        if (uploadResult.IsFailed)
        {
            return Result<Error>.Failure(uploadResult.Error!.Value);
        }

        Send(receiver.Endpoint, buffer);

        return Result<Error>.Success();
    }
}
