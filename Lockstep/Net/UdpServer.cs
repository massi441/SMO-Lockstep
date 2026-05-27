namespace Lockstep.Net;

using System.Net;
using System.Net.Sockets;
using Lockstep.Protocol;
using Lockstep.Util;
using Microsoft.Extensions.Logging;

internal class UdpServer
{
    private readonly int _port;
    private readonly ILogger _logger;

    public UdpServer(int port, ILogger logger)
    {
        _port = port;
        _logger = logger;
    }

    public void Run()
    {
        using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        IPEndPoint listenEndpoint = new IPEndPoint(IPAddress.Any, _port);

        socket.Bind(listenEndpoint);

        RunLoop(socket);
    }

    private void RunLoop(Socket socket)
    {
        _logger.LogInformation("SMO Lockstep server listening on port {Port}...", _port);

        while (true)
        {
            byte[] buffer = new byte[1024];
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint senderRef = sender;

            int bytesReceived = socket.ReceiveFrom(buffer, 10, SocketFlags.None, ref senderRef);

            if (bytesReceived > 0)
            {
                Packet packet = new Packet(buffer, bytesReceived ,sender);
                Result<Error> result = PacketDispatcher.Dispatch(packet);

                if (result.IsFailed)
                {
                    _logger.LogError("An error occured while processing the pack. Sender: {Address}:{Port}, Error: {Error}", sender.Address, sender.Port, result.Error);
                }
            }
            else
            {
                _logger.LogInformation("Empty packet received from {Address}:{Port}", sender.Address.ToString(), sender.Port);
            }
        }
    }
}
