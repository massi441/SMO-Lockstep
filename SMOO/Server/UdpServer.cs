using SMOO.Protocol;
using SMOO.Util;
using Microsoft.Extensions.Logging;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;
using SMOO.Services.Impl;

namespace SMOO.Server;

internal class UdpServer
{
    private readonly int _port;
    private readonly Channel<Packet> _packets;

    public ServerContext _context { get; private set; } = null!;

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

        _context.Logger.LogInformation("Server listening on port {Port}...", _port);

        try
        {
            await Task.WhenAll(
                ReceiveLoop(socket, cancellationToken),
                ProcessLoop(cancellationToken)
            );
        }
        catch (OperationCanceledException)
        {
            _context.Logger.LogWarning("Operations canceled.");
        }

        _context.Logger.LogInformation("Shutting down server...");

        await _context.RoomHolder.ShutdownRooms();
    }

    private async Task ReceiveLoop(Socket socket, CancellationToken cancellationTokenSource)
    {
        while (!cancellationTokenSource.IsCancellationRequested)
        {
            RentedBuffer buffer = new RentedBuffer(Config.MaxBufferSize);
            try
            {
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);

                SocketReceiveFromResult receiveResult = await socket.ReceiveFromAsync(buffer.RentRef, SocketFlags.None, sender, cancellationTokenSource);
                if (receiveResult.ReceivedBytes > 0)
                {
                    buffer.Restrict(receiveResult.ReceivedBytes);
                    _packets.Writer.TryWrite(new Packet
                    {
                        Sender = (IPEndPoint)receiveResult.RemoteEndPoint,
                        RentedBuffer = buffer
                    });
                    // ownership transferred to RentedBuffer
                }
                else
                {
                    buffer.Return();
                    _context.Logger.LogInformation("Empty packet received from {Address}:{Port}", sender.Address.ToString(), sender.Port);
                }
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.OperationAborted)
            {
                buffer.Return();
                _context.Logger.LogWarning("Operation aborted");
                break;
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.MessageSize)
            {
                buffer.Return();
                _context.Logger.LogError("The received packet was too big to fit inside the receive buffer");
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionReset)
            {
                buffer.Return();
                _context.Logger.LogWarning("An error occured while trying to send a packet, host unreachable");
            }
            catch (Exception ex)
            {
                buffer.Return();
                _context.Logger.LogError(ex, "An Unexpected exception occured while receiving packets");
            }
        }
    }
    private async Task ProcessLoop(CancellationToken cancellationTokenSource)
    {
        await foreach (Packet packet in _packets.Reader.ReadAllAsync(cancellationTokenSource))
        {
            Result<Error> dispatchResult = PacketDispatcher.Dispatch(packet, _context);
            if (dispatchResult.IsFailed)
            {
                _context.Logger.LogWarning("Dispatch failed. Error: {Error}, Sender: {Address}:{Port}", dispatchResult.Error, packet.Sender.Address, packet.Sender.Port);
                packet.RentedBuffer.Return();
            }
        }
    }

    private void InitContext(Socket socket, CancellationToken cancellationToken)
    {
        _context = new ServerContext()
        {
            Logger = LockstepLogger.Instance(),
            RoomHolder = new RoomHolder(),
            PacketSender = new PacketSenderUdp(socket),
            PlayerDisconnector = new PlayerDisconnector(),
            CancellationToken = cancellationToken
        };

        _context.RoomHolder.AddRoom(_context);
    }
}
