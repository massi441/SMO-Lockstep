using Lockstep.Net;
using Microsoft.Extensions.Logging;

namespace Lockstep.Server;

internal class ServerContext
{
    /// <summary>
    /// The logger used across the server
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// The room holder used across the server
    /// </summary>
    public IRoomHolder RoomHolder { get; }

    /// <summary>
    /// The packet sender used across the server
    /// </summary>
    public IPacketSender PacketSender { get; }

    /// <summary>
    /// The cancellation used to signal a server shutdown
    /// </summary>
    public CancellationToken CancellationToken { get; }

    public ServerContext(ILogger logger, IRoomHolder roomHolder, IPacketSender packetSender, CancellationToken cancellationToken, bool addDefaultRoom = true)
    {
        Logger = logger;
        RoomHolder = roomHolder;
        PacketSender = packetSender;
        CancellationToken = cancellationToken;

        if (addDefaultRoom)
        {
            RoomHolder.AddRoom(this);
        }
    }
}
