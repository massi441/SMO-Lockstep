namespace Lockstep.Server;

internal interface IRoomHolder
{
    uint AddRoom(ServerContext context);
    bool RemoveRoom(uint id);
    Room? GetRoom(uint id);
    Task ShutdownRooms();
    IEnumerable<Room> GetRooms();
}
