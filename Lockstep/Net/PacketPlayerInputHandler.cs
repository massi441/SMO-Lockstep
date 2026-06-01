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

    public void Handle(Packet packet, Room room)
    {
        room.Broadcaster.BroadcastExcept(room, packet.Sender, packet.Payload.Buffer.Span);
    }
}
