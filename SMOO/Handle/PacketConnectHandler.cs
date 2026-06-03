using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using SMOO.Client;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Util;
using Microsoft.Extensions.Logging;

namespace SMOO.Handle;

internal class PacketConnectHandler : IPacketHandler
{
    private readonly ServerContext _context;

    /// <summary>
    /// Requires at least one UInt16 for the length of the Player's name
    /// </summary>
    public uint MinPayloadSize => 2;

    public PacketConnectHandler(ServerContext context)
    {
        _context = context;
    }

    /// <summary>
    /// The packet sent by clients who request to connect to a room
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private readonly ref struct PacketConnectPayload
    {
        private readonly ReadOnlySpan<byte> _buffer;

        /// <summary>
        /// The length of the player's name, starting at offset 0x0
        /// </summary>
        public readonly byte NameLength => _buffer[0];

        /// <summary>
        /// The name of the player, starting at offset 0x1
        /// </summary>
        public readonly string Name => Encoding.UTF8.GetString(_buffer.Slice(0x1, NameLength));

        public PacketConnectPayload(ReadOnlySpan<byte> buffer)
        {
            _buffer = buffer;
        }
    }

    /// <summary>
    /// The packet sent to a player that just connected to a room
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct PacketConnectAck
    {
        public required PacketHeader Header;
        public ushort SequenceNumber;
        public required ushort RoomSize;

        public static ushort SizeOf()
        {
            return (ushort)Unsafe.SizeOf<PacketConnectAck>();
        }
    }

    public void Handle(Packet packet, Room room)
    {
        if (IsInOtherRoom(packet.Sender, out Player takenRoomPlayer, out Room takenRoom))
        {
            _context.Logger.LogWarning("Player {Name} ({Address}:{Port}) is already in room {RoomId}", takenRoomPlayer.Name, takenRoomPlayer.Endpoint.Address, takenRoomPlayer.Endpoint.Port, takenRoom.Id);
            return;
        }

        PacketConnectPayload connectPayload = new PacketConnectPayload(packet.Payload);

        if (!IsValidNameLength(connectPayload.NameLength))
        {
            _context.Logger.LogWarning("Invalid player name length {Length}", connectPayload.NameLength);
            return;
        }

        PlayerInfo playerInfo = new PlayerInfo()
        {
            Endpoint = packet.Sender,
            Name = connectPayload.Name,
            Room = room,
        };

        Result<Player, Error> registerResult = room.PlayerHolder.RegisterPlayer(playerInfo);
        if (registerResult.IsFailed)
        {
            _context.Logger.LogError("Failed to register {PlayerName} in Room #{RoomId}", playerInfo.Name, room.Id);
            return;
        }

        Player newPlayer = registerResult.Data!;

        RentedBuffer packetBuffer = MemoryUtil.Rent<PacketConnectAck>();
        packetBuffer.Write(new PacketConnectAck()
        {
            Header = packet.Header.WithSizeType(MemoryUtil.PayloadSize<PacketConnectAck>(), PacketType.ConnectAck),
            RoomSize = room.PlayerHolder.MaxSize
        });

        PendingPacketRequest request = new PendingPacketRequest()
        {
            Receiver = registerResult.Data!,
            RentedPayload = packetBuffer,
        };

        _context.PacketSender.Send(newPlayer.Endpoint, packetBuffer.Span);

        room.Broadcaster.PendingPacketStore.UploadPacket(request);

        _context.Logger.LogTrace("Player {Name} joined #{RoomId}, waiting for a confirmation...", newPlayer.Name, packet.Header.RoomId);
    }

    private bool IsInOtherRoom(IPEndPoint sender, out Player player, out Room takenRoom)
    {
        foreach (Room room in _context.RoomHolder.GetRooms())
        {
            Player? p = room.PlayerHolder.FindPlayerByHost(sender);
            if (p != null)
            {
                player = p;
                takenRoom = room;
                return true;
            }
        }

        player = null!;
        takenRoom = null!;

        return false;
    }

    private static bool IsValidNameLength(int nameLength)
    {
        return nameLength > 0 && nameLength <= Config.MaxPlayerNameLength;
    }
}
