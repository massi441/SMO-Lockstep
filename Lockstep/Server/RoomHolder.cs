using Lockstep.Client;

namespace Lockstep.Server;

internal class RoomHolder : IRoomHolder
{
    private readonly Dictionary<ushort, Room> _rooms = [];

    public ushort AddRoom(ServerContext context)
    {
        ushort nextId = 0;

        if (_rooms.Count > 0)
        {
            nextId = (ushort)(_rooms.Keys.Max() + 1); 
        }

        Room room = new Room(nextId, context, new PlayerHolder());
        room.Start();
        _rooms.Add(nextId, room);
        return nextId;
    }

    public bool RemoveRoom(ushort id)
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

    public Room? GetRoom(ushort id)
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

    public IEnumerable<Room> GetRooms()
    {
        return _rooms.Values;
    }
}
