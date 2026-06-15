using System.Net;
using System.Net.Sockets;
using SMOO.Client;
using SMOO.Protocol;
using SMOO.Server;
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

    public void SendReliably(Player receiver, RentedBuffer buffer, Room room, RefCounter refCounter, byte maxRetries = 5)
    {
        room.Broadcaster.ReliablePacketStore.UploadPacket(buffer, refCounter, receiver, maxRetries);

        Send(receiver.Endpoint, buffer);
    }

    public void SendReliably(Player receiver, RentedBuffer buffer, Room room, byte maxRetries = 5)
    {
        SendReliably(receiver, buffer, room, new RefCounter(), maxRetries);
    }
}
