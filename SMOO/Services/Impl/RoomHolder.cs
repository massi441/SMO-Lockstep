using SMOO.Client;
using SMOO.Server;
using SMOO.Services.Interface;

namespace SMOO.Services.Impl;

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

        IPlayerHolder playerHolder = new PlayerHolder();
        IBroadcaster roomBroadcaster = new Broadcaster(context, new ReliablePacketStore(context));

        _rooms.Add(nextId, new Room(nextId, context, playerHolder, roomBroadcaster));

        return nextId;
    }

    public async Task<bool> RemoveRoom(ushort id)
    {
        if (_rooms.TryGetValue(id, out Room? room))
        {
            if (room != null)
            {
                await room.Shutdown();
                _rooms.Remove(id);
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
        return Task.WhenAll(_rooms.Values.Select(room => room.Shutdown()));
    }

    public IEnumerable<Room> GetRooms()
    {
        return _rooms.Values;
    }
}
