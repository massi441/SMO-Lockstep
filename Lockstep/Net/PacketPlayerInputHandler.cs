using Lockstep.Protocol;
using Lockstep.Server;

namespace Lockstep.Net;

internal class PacketPlayerInputHandler : IPacketHandler
{
    private readonly ServerContext _context;

    public PacketPlayerInputHandler(ServerContext context)
    {
        _context = context;
    }

    public uint MinPayloadSize => 0;

    public void Handle(ParsedPacket packet, Room room)
    {
        room.Broadcaster.Broadcast(room, packet.RentedBuffer.Ref);
    }
}
