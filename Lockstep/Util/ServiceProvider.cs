using System.Net.Sockets;
using Lockstep.Client;
using Lockstep.Net;
using Microsoft.Extensions.Logging;

namespace Lockstep.Util;

internal class ServiceProvider
{
    public ILogger Logger { get; }
    public IClientHolder ClientHolder { get; } // TODO: Use interface
    public IPacketSender PacketSender { get; }

    public ServiceProvider(ILogger logger, IClientHolder holder, IPacketSender packetSender)
    {
        Logger = logger;
        ClientHolder = holder;
        PacketSender = packetSender;
    }
}
