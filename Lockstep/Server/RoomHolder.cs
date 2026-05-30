using Lockstep.Client;

namespace Lockstep.Server;

internal class RoomHolder : IRoomHolder
{
    private readonly Dictionary<uint, Room> _rooms = [];

    public uint AddRoom(ServerContext context)
    {
        uint nextId = 0;

        if (_rooms.Count > 0)
        {
            nextId = _rooms.Keys.Max() + 1; 
        }

        Room room = new Room(nextId, context, new PlayerHolder());
        room.Start();
        _rooms.Add(nextId, room);
        return nextId;
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
        return Task.WhenAll(_rooms.Values.Select(room =>
        {
            room.Shutdown();
            return room.Task;
        }));
    }
}
