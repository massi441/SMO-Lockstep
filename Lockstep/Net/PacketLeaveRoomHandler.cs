using System.Runtime.InteropServices;
using Lockstep.Client;
using Lockstep.Protocol;
using Lockstep.Server;
using Lockstep.Util;
using Microsoft.Extensions.Logging;

namespace Lockstep.Net;

internal class PacketLeaveRoomHandler : IPacketHandler
{
    private readonly ServerContext _context;
    public uint MinPayloadSize => 0;

    public PacketLeaveRoomHandler(ServerContext context)
    {
        _context = context;
    }

    public Result<Error> Handle(Packet packet, Room room)
    {
        PacketHeader header = new PacketHeader
        {
            Version = packet.Header.Version,
            RoomId = room.Id
        };

        Span<byte> buffer = stackalloc byte[PacketHeader.SizeOf()];

        MemoryMarshal.Write(buffer, PacketHeader.Magic);

        foreach (Player player in room.PlayerHolder.GetPlayers())
        {
            if (player.Info.Endpoint.Equals(packet.Sender))
            {
                Result<Error> result = room.PlayerHolder.UnregisterPlayer(player);
                if (result.IsFailed)
                {
                    return Result<Error>.Failure(result.Error!.Value);
                }

                _context.Logger.LogWarning("Player {PlayerName} left room {Roomid}", player.Info.Name, room.Id);

                header.Type = PacketType.Ack;
            }
            else
            {
                header.Type = PacketType.LeaveRoom;
            }

            MemoryMarshal.Write(buffer[PacketHeader.SizeOfMagic()..], header);

            _context.PacketSender.Send(buffer, player.Info.Endpoint);
        }

        return Result<Error>.Success();
    }
}
