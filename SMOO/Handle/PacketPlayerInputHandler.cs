using SMOO.Protocol;
using SMOO.Server;

namespace SMOO.Handle;

internal class PacketPlayerInputHandler : IPacketHandler
{
    private readonly ServerContext _context;

    public PacketPlayerInputHandler(ServerContext context)
    {
        _context = context;
    }

    public uint MinPayloadSize => 0;

    public void Handle(Packet packet, Room room)
    {
        room.Broadcaster.Broadcast(room, packet.RentedBuffer.Span);
    }
}
