using Lockstep.Net;
using Lockstep.Protocol;
using Lockstep.Services;
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
    private readonly Channel<Packet> _packets;

    private ILogger Logger => Context.Logger;
    public ServerContext Context { get; private set; } = null!;

    public UdpServer(int port)
    {
        _port = port;
        _packets = Channel.CreateUnbounded<Packet>();
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        IPEndPoint listenEndpoint = new IPEndPoint(IPAddress.Any, _port);

        socket.Bind(listenEndpoint);

        InitContext(socket, cancellationToken);

        Logger.LogInformation("Server listening on port {Port}...", _port);

        try
        {
            await Task.WhenAll(
                ReceiveLoop(socket, cancellationToken),
                ProcessLoop(cancellationToken)
            );
        }
        catch (OperationCanceledException)
        {
            Logger.LogWarning("Operations canceled.");
        }

        Logger.LogInformation("Shutting down server...");

        await Context.RoomHolder.ShutdownRooms();
    }

    private async Task ReceiveLoop(Socket socket, CancellationToken cancellationTokenSource)
    {
        while (!cancellationTokenSource.IsCancellationRequested)
        {
            byte[] buffer = ArrayPool<byte>.Shared.Rent(Config.ServerBufferSize);
            try
            {
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);

                SocketReceiveFromResult receiveResult = await socket.ReceiveFromAsync(buffer, SocketFlags.None, sender, cancellationTokenSource);
                if (receiveResult.ReceivedBytes > 0)
                {
                    _packets.Writer.TryWrite(new Packet
                    {
                        Sender = (IPEndPoint)receiveResult.RemoteEndPoint,
                        RentedBuffer = new RentedBuffer<byte>(buffer, receiveResult.ReceivedBytes)
                    });
                }
                else
                {
                    Logger.LogInformation("Empty packet received from {Address}:{Port}", sender.Address.ToString(), sender.Port);
                }
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.OperationAborted)
            {
                Logger.LogWarning("Operation aborted");
                break;
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.MessageSize)
            {
                Logger.LogError("The received packet was too big to fit inside the receive buffer");
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionReset)
            {
                Logger.LogWarning("An error occured while trying to send a packet");
            }
            catch (SocketException ex)
            {
                Logger.LogError(ex, "An Unexpected exception occured while receiving packets");
            }
        }
    }
    private async Task ProcessLoop(CancellationToken cancellationTokenSource)
    {
        await foreach (Packet packet in _packets.Reader.ReadAllAsync(cancellationTokenSource))
        {
            Result<Error> dispatchResult = PacketDispatcher.Dispatch(packet, Context);
            if (dispatchResult.IsFailed)
            {
                Logger.LogWarning("Dispatch failed. Error: {Error}, Sender: {Address}:{Port}", dispatchResult.Error, packet.Sender.Address, packet.Sender.Port);
            }
        }
    }

    private void InitContext(Socket socket, CancellationToken cancellationToken)
    {
        Context = new ServerContext()
        {
            Logger = LockstepLogger.Instance(),
            RoomHolder = new RoomHolder(),
            PacketSender = new PacketSenderUdp(socket),
            PlayerDisconnector = new PlayerDisconnector(),
            CancellationToken = cancellationToken
        };

        Context.RoomHolder.AddRoom(Context);
    }
}
