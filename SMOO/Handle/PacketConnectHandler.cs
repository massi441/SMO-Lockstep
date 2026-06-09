using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Logging;
using SMOO.Client;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Util;

namespace SMOO.Handle;

internal static class PacketConnectHandler
{
    /// <summary>
    /// Requires at least one UInt16 for the length of the Player's name
    /// </summary>
    public static ushort MinPayloadSize => 2;

    /// <summary>
    /// The payload sent by a client who requests to connect to a room
    /// </summary>
    private struct PacketConnectPayload : IDeserializableStruct
    {
        /// <summary>
        /// The length of the player's name, starting at offset 0x0
        /// </summary>
        public byte NameLength { get; private set; }

        /// <summary>
        /// The name of the player, starting at offset 0x1
        /// </summary>
        public string Name { get; private set; }

        public void Deserialize(ReadOnlySpan<byte> source)
        {
            NameLength = source[0];
            Name = Encoding.UTF8.GetString(source.Slice(0x1, NameLength));
        }
    }

    /// <summary>
    /// The packet sent to a player that just connected to a room
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct PacketConnectAck : ISerializableStruct
    {
        public required PacketHeader Header;
        public ushort SequenceNumber;
        public required ClientInfo ClientInfo;

        public static ushort SizeOf()
        {
            return (ushort)Unsafe.SizeOf<PacketConnectAck>();
        }

        public readonly void Serialize(Span<byte> destination)
        {
            MemoryMarshal.Write(destination, this);
        }
    }

    public static void Handle(ParsedPacket packet, Room room, ServerContext context)
    {
        if (IsInOtherRoom(packet.SenderIp, context, out Player? takenPlayer, out Room takenRoom))
        {
            context.Logger.LogWarning("Player {Name} ({Address}:{Port}) is already in room {RoomId}", takenPlayer.Name, takenPlayer.Endpoint.Address, takenPlayer.Endpoint.Port, takenRoom.Id);
            goto cleanup;
        }

        PacketConnectPayload connectPayload = PacketSerializer.Deserialize<PacketConnectPayload>(packet.Payload);

        if (!IsValidNameLength(connectPayload.NameLength))
        {
            context.Logger.LogWarning("Invalid player name length {Length}", connectPayload.NameLength);
            goto cleanup;
        }

        PlayerInfo playerInfo = new PlayerInfo()
        {
            Endpoint = packet.SenderIp,
            Name = connectPayload.Name,
            Room = room,
        };

        Result<Player, Error> newPlayerResult = room.PlayerHolder.RegisterPlayer(in playerInfo);
        if (newPlayerResult.IsFailed)
        {
            context.Logger.LogError("Failed to register {PlayerName} in Room #{RoomId}", playerInfo.Name, room.Id);
            goto cleanup;
        }

        Player newPlayer = newPlayerResult.Data!;

        if (AckConnect(newPlayer, ref packet, room, context)) // ownership of buffer gets transferred to reliable store
        {
            context.Logger.LogTrace("Player {Name} joined Room #{RoomId}, waiting for a confirmation...", newPlayer.Name, packet.Header.RoomId);
            return;
        }

        context.Logger.LogError("Failed to upload connect ACK packet, new player will be ignored");

        cleanup:
        packet.RentedBuffer.Return();
    }

    private static bool AckConnect(Player newPlayer, ref ParsedPacket packet, Room room, ServerContext context)
    {
        PacketConnectAck ackPacket = new PacketConnectAck()
        {
            Header = packet.Header.WithSizeType(MemoryUtil.PayloadSize<PacketConnectAck>(), PacketType.ConnectAck),
            ClientInfo = new ClientInfo()
            {
                RoomSize = room.PlayerHolder.MaxSize,
                SessionId = newPlayer.Id.SessionId,
                ActivePlayerCount = room.PlayerHolder.ActivePlayerCount
            }
        };

        RentedBuffer ackBuffer = MemoryUtil.Rent<PacketConnectAck>();
        ackPacket.Serialize(ackBuffer.UsedSpan);

        Result<Error> uploadResult = room.Broadcaster.ReliablePacketStore.UploadPacket(ackBuffer, new AtomicCounter(), newPlayer);
        if (uploadResult.IsFailed)
        {
            return false;
        }

        context.PacketSender.Send(newPlayer.Endpoint, ackBuffer.UsedSpan);

        return true;
    }

    private static bool IsInOtherRoom(IPEndPoint sender, ServerContext context, out Player player, out Room takenRoom)
    {
        foreach (Room room in context.RoomHolder.GetRooms())
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
