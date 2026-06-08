using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Logging;
using SMOO.Client;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Util;

namespace SMOO.Handle;

internal class PacketChatMessageHandler : IPacketHandler
{
    private readonly ServerContext _context;

    public PacketChatMessageHandler(ServerContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Requires the size of the chat message to be provided
    /// </summary>
    public uint MinPayloadSize => 0;

    public void Handle(Packet packet, Room room, Player? player)
    {
        if (packet.Payload.Length == 0)
        {
            _context.Logger.LogTrace("Empty Message received in room #{RoomId}, skipping broadcast", room.Id);
            return;
        }

        string message = Encoding.UTF8.GetString(packet.Payload);
        _context.Logger.LogTrace("{PlayerName} sent a message in room #{RoomId}: {Message}", player!.Name, room.Id, message);

        room.Broadcaster.BroadcastReliablyExcept(room, player, new ReliablePacketBroadcastRequest()
        {
            MaxRetries = Config.MaxRetries,
            RentedPayload = packet.RentedBuffer
        });
    }
}
