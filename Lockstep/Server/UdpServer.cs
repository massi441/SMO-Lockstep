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
    private const int BufferSize = 1024;

    private readonly int _port;
    private readonly Channel<ServerJob> _jobs;

    private ILogger Logger => Context.Logger;
    public ServerContext Context { get; private set; } = null!;

    public UdpServer(int port)
    {
        _port = port;
        _jobs = Channel.CreateUnbounded<ServerJob>();
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
                ProcessLoop(socket, cancellationToken)
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
            byte[] buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
            try
            {
                await ReceiveNext(socket, buffer, cancellationTokenSource);
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
    private async Task ProcessLoop(Socket socket, CancellationToken cancellationTokenSource)
    {
        await foreach (ServerJob workItem in _jobs.Reader.ReadAllAsync(cancellationTokenSource))
        {
            try
            {
                ref Payload packet = ref workItem.Packet;

                Result<Error> dispatchResult = PacketDispatcher.Dispatch(packet, Context);
                if (dispatchResult.IsFailed)
                {
                    Logger.LogWarning("Packet rejected. Sender: {Address}:{Port}, Error: {Error}", packet.Sender.Address, packet.Sender.Port, dispatchResult.Error);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(workItem.RentedBuffer);
            }
        }
    }

    private async Task ReceiveNext(Socket socket, byte[] buffer, CancellationToken cancellationTokenSource)
    {
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);

        SocketReceiveFromResult receiveResult = await socket.ReceiveFromAsync(buffer, SocketFlags.None, sender, cancellationTokenSource);
        if (receiveResult.ReceivedBytes > 0)
        {
            ServerJob workItem = new ServerJob()
            {
                Packet = new Payload(buffer.AsMemory()[..receiveResult.ReceivedBytes], (IPEndPoint)receiveResult.RemoteEndPoint),
                RentedBuffer = buffer
            };

            _jobs.Writer.TryWrite(workItem);
        }
        else
        {
            Logger.LogInformation("Empty packet received from {Address}:{Port}", sender.Address.ToString(), sender.Port);
        }
    }

    private void InitContext(Socket socket, CancellationToken cancellationToken)
    {
        ILogger logger = LockstepLogger.Instance();
        IRoomHolder roomHolder = new RoomHolder();
        IPacketSender sender = new UdpPacketSender(socket);

        Context = new ServerContext(logger, roomHolder, sender, cancellationToken);
    }
}
