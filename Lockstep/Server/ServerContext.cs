using Lockstep.Net;
using Microsoft.Extensions.Logging;

namespace Lockstep.Server;

internal class ServerContext
{
    public ILogger Logger { get; }
    public IRoomHolder RoomHolder { get; }
    public IPacketSender PacketSender { get; }

    public ServerContext(ILogger logger, IRoomHolder roomHolder, IPacketSender packetSender, bool addDefaultRoom = true)
    {
        Logger = logger;
        RoomHolder = roomHolder;
        PacketSender = packetSender;

        if (addDefaultRoom)
        {
            RoomHolder.AddRoom(this);
        }
    }
}
