using System.Text;
using Lockstep.Client;
using Lockstep.Protocol;
using Lockstep.Server;
using Lockstep.Util;

namespace Lockstep.Net;

internal class PacketPlayerInputHandler : IPacketHandler
{
    private readonly ServerContext _context;

    public PacketPlayerInputHandler(ServerContext context)
    {
        _context = context;
    }

    public uint MinPayloadSize => 0;

    public Result<Error> Handle(Packet packet, Room room)
    {
        ReadOnlySpan<Player> players = room.PlayerHolder.GetPlayers();

        foreach (Player player in players)
        {
            string message = "Relayed Player inputs!";
            _context.PacketSender.Send(Encoding.UTF8.GetBytes(message), player.Info.Endpoint);
        }

        return Result<Error>.Success();
    }
}
