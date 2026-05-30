namespace Lockstep.Server;

internal interface IRoomHolder
{
    ushort AddRoom(ServerContext context);
    bool RemoveRoom(ushort id);
    Room? GetRoom(ushort id);
    Task ShutdownRooms();
    IEnumerable<Room> GetRooms();
}
