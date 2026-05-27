using Lockstep.Client;
using Lockstep.Net;
using Microsoft.Extensions.Logging;

namespace Lockstep.Util;

internal class ServiceProvider
{
    public ILogger Logger { get; }
    public ClientHolder ClientHolder { get; } // TODO: Use interface
    public IPacketSender PacketSender { get; }

    public ServiceProvider()
    {
        Logger = LockstepLogger.Instance();
        ClientHolder = new ClientHolder();
        PacketSender = null!;
    }
}
