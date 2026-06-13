using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using SMOO.Client;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Util;

namespace SMOO.Handle;

internal class PacketConnectHandler : IPacketHandler
{
    /// <summary>
    /// Requires at least one UInt16 for the length of the Player's name
    /// </summary>
    public static ushort MinPayloadSize => 2;

    private struct PacketConnectPayload : IDeserializableStruct
    {
        public byte NameLength { get; private set; }
        public string Name { get; private set; }

        public void Deserialize(ref SpanReader reader)
        {
            NameLength = reader.ReadByte();
            Name = Encoding.UTF8.GetString(reader.RemainingSpan);
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

        if (AckConnect(newPlayer, ref packet, room, context))
        {
            context.Logger.LogTrace("Player {Name} joined Room #{RoomId} in slot {Slot}, waiting for a confirmation...", newPlayer.Name, packet.Header.RoomId, newPlayer.Slot);
            goto cleanup;
        }

        context.Logger.LogError("Failed to upload connect ACK packet, new player will be ignored");

        cleanup:
        packet.RentedBuffer.Return();
    }

    private static bool AckConnect(Player newPlayer, ref ParsedPacket packet, Room room, ServerContext context)
    {
        PacketHeader header = packet.Header.WithType(PacketType.ConnectAck);

        PlayerInRoomInfo[] playerInfos = [.. room.PlayerHolder.Players.Select(p => new PlayerInRoomInfo(p))];

        PacketConnectAck ackPacket = new PacketConnectAck()
        {
            Header = header,
            RoomSize = room.PlayerHolder.MaxSize,
            SessionId = newPlayer.Id.SessionId,
            OtherPlayersCount = (byte)(room.PlayerHolder.ActivePlayerCount - 1),
            PlayerInfos = playerInfos
        };

        RentedBuffer ackBuffer = new RentedBuffer(ackPacket.FinalizeSize());

        ackPacket.Serialize(ackBuffer.UsedSpan);

        Result<Error> uploadResult = room.Broadcaster.ReliablePacketStore.UploadPacket(ackBuffer, new RefCounter(), newPlayer);
        if (uploadResult.IsFailed)
        {
            return false;
        }

        context.PacketSender.SendTo(newPlayer.Endpoint, ackBuffer.UsedSpan);

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
