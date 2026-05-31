using System.Text;
using Lockstep.Client;
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
        foreach (Player player in room.PlayerHolder.Players)
        {
            string message = "Relayed Player inputs!";
            _context.PacketSender.Send(player.Endpoint, Encoding.UTF8.GetBytes(message));
        }
    }
}
