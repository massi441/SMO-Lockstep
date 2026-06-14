using System.Net;
using Microsoft.Extensions.Logging;
using SMOO.Client;
using SMOO.Enumerator;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Util;

namespace SMOO.Handle;

internal class PacketConnectHandler : IPacketHandler
{
    /// <summary>
    /// Requires at least one UInt16 for the length of the Player's name
    /// </summary>
    public static ushort MinPayloadSize => RequiredSize<PacketConnectPayload>.MinSize;
    public static ushort MaxPayloadSize => RequiredSize<PacketConnectPayload>.MaxSize;

    private struct PacketConnectPayload : IDeserializableStruct
    {
        [DynamicField(MaxSize = Config.MaxPlayerNameLength)]
        public StreamStringView<byte> Name;

        public void Deserialize(ref SpanReader reader)
        {
            Name.Deserialize(ref reader, Config.MaxPlayerNameLength);    
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

        if (!IsValidNameLength(connectPayload.Name.Length))
        {
            context.Logger.LogWarning("Invalid player name length {Length}", connectPayload.Name.Length);
            goto cleanup;
        }

        PlayerInfo playerInfo = new PlayerInfo()
        {
            Endpoint = packet.SenderIp,
            Name = connectPayload.Name.String,
            Room = room,
        };

        Result<Player, Error> newPlayerResult = room.PlayerHolder.RegisterPlayer(in playerInfo);
        if (newPlayerResult.IsFailed)
        {
            context.Logger.LogError("Failed to register {PlayerName} in Room #{RoomId}", playerInfo.Name, room.Id);
            goto cleanup;
        }

        Player newPlayer = newPlayerResult.Data!;

        var playerInfos = room.PlayerHolder.Players.PlayerInfosExcept(newPlayer);

        PacketConnectAck ackPacket = new PacketConnectAck()
        {
            Header = packet.Header.WithType(PacketType.ConnectAck),
            RoomSize = room.PlayerHolder.MaxSize,
            SessionId = newPlayer.Id.SessionId,
            OtherPlayersCount = (byte)(room.PlayerHolder.Players.ActiveCount() - 1),
            PlayerInfos = playerInfos
        };

        RentedBuffer ackBuffer = PacketSerializer.Serialize(ref ackPacket, Config.MaxBufferSize);

        Result<Error> ackResult = context.PacketSender.SendReliably(newPlayer, ackBuffer, room.Broadcaster.ReliablePacketStore, Config.MaxRetries);
        if (ackResult.IsSuccess)
        {
            context.Logger.LogTrace("Player {Name} joined Room #{RoomId} in slot {Slot}, waiting for a confirmation...", newPlayer.Name, packet.Header.RoomId, newPlayer.Slot);
        }
        else
        {
            context.Logger.LogError("Failed to upload connect ACK packet, new player will be ignored");
            room.PlayerHolder.UnregisterPlayer(newPlayer);
            ackBuffer.Return();
        }

    cleanup:
        packet.RentedBuffer.Return();
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
