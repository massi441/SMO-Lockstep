using Lockstep.Client;
using Lockstep.Net;
using Lockstep.Server;
using Microsoft.Extensions.Logging;

namespace Lockstep.Util;

internal class ServiceProvider
{
    public ILogger Logger { get; }
    public IPacketSender PacketSender { get; }
    public Dictionary<uint, Room> Rooms { get; } = [];

    public ServiceProvider(ILogger logger, IPacketSender packetSender)
    {
        Logger = logger;
        PacketSender = packetSender;
    }

    public void AddRoom()
    {
        if (Rooms.Count > 0)
        {
            Rooms.Add(Rooms.Keys.Max() + 1, new Room(null!));
        }
        else
        {
            Rooms.Add(0, new Room(null!));
        }
    }

    public bool RemoveRoom(uint id)
    {
        return Rooms.Remove(id);
    }
}
