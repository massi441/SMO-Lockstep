using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SMOO.Client;
using SMOO.Net;
using SMOO.Protocol;
using SMOO.Util;

namespace SMOO.Services;

internal class PlayerDisconnector : IPlayerDisconnector
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct PacketPlayerLeaveRoom
    {
        public required PacketHeader Header;
        public ushort SequenceNumber;
        public required byte PlayerPort;

        public static int SizeOf()
        {
            return Unsafe.SizeOf<PacketPlayerLeaveRoom>();
        }

        public static ushort SizeOfPayload()
        {
            return sizeof(byte);
        }
    }

    public Result<Error> Disconnect(Player player)
    {
        byte[] broadcastBuffer = ArrayPool<byte>.Shared.Rent(PacketPlayerLeaveRoom.SizeOf());

        PacketHeader header = new PacketHeader()
        {
            Type = PacketType.PlayerLeaveRoom,
            Flags = (byte)PacketFlags.None,
            Version = Config.Version,
            RoomId = player.Room.Id,
            PayloadSize = PacketPlayerLeaveRoom.SizeOfPayload()
        };

        PacketPlayerLeaveRoom leavePacket = new PacketPlayerLeaveRoom()
        {
            Header = header,
            PlayerPort = player.PortNumber
        };

        MemoryMarshal.Write(broadcastBuffer, leavePacket);

        PacketAckBroadcastRequest request = new PacketAckBroadcastRequest()
        {
            MaxRetries = Config.MaxRetries,
            Payload = broadcastBuffer
        };

        Result<Error> unregisterResult = player.Room.PlayerHolder.UnregisterPlayer(player);
        if (unregisterResult.IsFailed)
        {
            return unregisterResult;
        }

        return player.Room.Broadcaster.BroadcastAck(player.Room, request);
    }
}
