namespace Lockstep.Net;

using System.Net;
using System.Net.Sockets;
using Lockstep.Client;
using Lockstep.Protocol;
using Lockstep.Util;
using Microsoft.Extensions.Logging;

internal class UdpServer
{
    private readonly int _port;
    private ServiceProvider _serviceProvider = null!;

    private ServiceProvider Provider => _serviceProvider;

    private ILogger Logger => _serviceProvider.Logger;

    public UdpServer(int port)
    {
        _port = port;
    }

    public void Run()
    {
        using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        IPEndPoint listenEndpoint = new IPEndPoint(IPAddress.Any, _port);

        socket.Bind(listenEndpoint);

        InitProvider(socket);
        RunLoop(socket);
    }

    private void RunLoop(Socket socket)
    {
        Logger.LogInformation("SMO Lockstep server listening on port {Port}...", _port);

        while (true)
        {
            byte[] buffer = new byte[1024];
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);

            try
            {
                ReceiveNext(socket, buffer, sender);
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.MessageSize)
            {
                Logger.LogError("The received packet was too big to fit inside the receive buffer");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "An Unexpected exception occured while receiving packets");
            }
        }
    }

    private void ReceiveNext(Socket socket, byte[] buffer, IPEndPoint sender)
    {
        EndPoint senderRef = sender;

        int bytesReceived = socket.ReceiveFrom(buffer, buffer.Length, SocketFlags.None, ref senderRef);
        if (bytesReceived > 0)
        {
            Payload packet = new Payload(buffer.AsSpan(0, bytesReceived), sender);
            Result<Error> result = PacketDispatcher.Dispatch(packet, _serviceProvider);

            if (result.IsFailed)
            {
                Logger.LogError("An error occured while processing the pack. Sender: {Address}:{Port}, Error: {Error}", sender.Address, sender.Port, result.Error);
            }
        }
        else
        {
            Logger.LogInformation("Empty packet received from {Address}:{Port}", sender.Address.ToString(), sender.Port);
        }
    }

    private void InitProvider(Socket socket)
    {
        ILogger logger = LockstepLogger.Instance();
        IClientHolder holder = new ClientHolder();
        IPacketSender sender = new UdpPacketSender(socket);

        _serviceProvider = new ServiceProvider(logger, holder, sender);
    }
}
