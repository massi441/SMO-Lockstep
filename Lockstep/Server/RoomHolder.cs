using Lockstep.Client;
using Lockstep.Util;

namespace Lockstep.Server;

internal class RoomHolder : IRoomHolder
{
    private readonly Dictionary<uint, Room> _rooms = [];

    public uint AddRoom(ServerContext context)
    {
        uint nextKey = 0;

        if (_rooms.Count > 0)
        {
            nextKey = _rooms.Keys.Max() + 1; 
        }

        _rooms.Add(nextKey, new Room(context, new ClientHolder()));
        return nextKey;
    }

    public bool RemoveRoom(uint id)
    {
        if (_rooms.TryGetValue(id, out Room? room))
        {
            if (room != null)
            {
                room.Shutdown();
                return true;
            }
        }

        return false;
    }

    public Room? GetRoom(uint id)
    {
        if (_rooms.TryGetValue(id, out Room? room))
        {
            return room;
        }

        return null;
    }

    public Task ShutdownRooms()
    {
        return Task.WhenAll(_rooms.Values.Select(async room =>
        {
            room.Shutdown();
            return room.Task;
        }));
    }
}
