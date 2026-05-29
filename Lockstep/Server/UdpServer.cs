using Lockstep.Net;
using Lockstep.Protocol;
using Lockstep.Util;
using Microsoft.Extensions.Logging;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;

namespace Lockstep.Server;

internal class UdpServer
{
    private readonly int _port;
    private readonly Channel<WorkItem> _packetChannel;
    private ServiceProvider _serviceProvider = null!;

    private const int BufferSize = 1024;

    private ILogger Logger => _serviceProvider.Logger;

    public UdpServer(int port)
    {
        _port = port;
        _packetChannel = Channel.CreateUnbounded<WorkItem>();
    }

    public async Task RunAsync(CancellationToken ct)
    {
        using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        IPEndPoint listenEndpoint = new IPEndPoint(IPAddress.Any, _port);

        socket.Bind(listenEndpoint);

        InitProvider(socket);

        await Task.WhenAll(
            ReceiveLoop(socket, ct),
            ProcessLoop(socket, ct)
        );
    }

    private async Task ReceiveLoop(Socket socket, CancellationToken ct)
    {
        Logger.LogInformation("SMO Lockstep server listening on port {Port}...", _port);

        while (!ct.IsCancellationRequested)
        {
            byte[] buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
            try
            {
                await ReceiveNext(socket, buffer, ct);
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.MessageSize)
            {
                Logger.LogError("The received packet was too big to fit inside the receive buffer");
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionReset)
            {
                Logger.LogWarning("The sender received an empty message");
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.OperationAborted)
            {
                Logger.LogWarning("Cancel Requested, shutting down server");
            }
            catch (SocketException ex)
            {
                Logger.LogError(ex, "An Unexpected exception occured while receiving packets");
            }
        }
    }
    private async Task ProcessLoop(Socket socket, CancellationToken ct)
    {
        await foreach (WorkItem workItem in _packetChannel.Reader.ReadAllAsync(ct))
        {
            try
            {
                ref Payload packet = ref workItem.Packet;

                Result<Error> dispatchResult = PacketDispatcher.Dispatch(packet, _serviceProvider);
                if (dispatchResult.IsSuccess)
                {
                    Logger.LogInformation("Work uploaded on main Channel");
                }
                else
                {
                    Logger.LogError("An error occured while processing the pack. Sender: {Address}:{Port}, Error: {Error}", packet.Sender.Address, packet.Sender.Port, dispatchResult.Error);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(workItem.RentedBuffer);
            }
        }
    }

    private async Task ReceiveNext(Socket socket, byte[] buffer, CancellationToken ct)
    {
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);

        SocketReceiveFromResult receiveResult = await socket.ReceiveFromAsync(buffer, SocketFlags.None, sender, ct);

        if (receiveResult.ReceivedBytes > 0)
        {
            WorkItem workItem = new WorkItem()
            {
                Packet = new Payload(buffer.AsMemory()[..receiveResult.ReceivedBytes], (IPEndPoint)receiveResult.RemoteEndPoint),
                RentedBuffer = buffer
            };

            _packetChannel.Writer.TryWrite(workItem);
        }
        else
        {
            Logger.LogInformation("Empty packet received from {Address}:{Port}", sender.Address.ToString(), sender.Port);
        }
    }

    private void InitProvider(Socket socket)
    {
        ILogger logger = LockstepLogger.Instance();
        IPacketSender sender = new UdpPacketSender(socket);

        _serviceProvider = new ServiceProvider(logger, sender);
        _serviceProvider.AddRoom();
    }
}
