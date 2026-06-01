using Lockstep.Server;

namespace Lockstep.Services;

internal interface IRoomHolder
{
    ushort AddRoom(ServerContext context);
    bool RemoveRoom(ushort id);
    Room? GetRoom(ushort id);
    Task ShutdownRooms();
    IEnumerable<Room> GetRooms();
}
