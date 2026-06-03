using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SMOO.Client;
using SMOO.Protocol;
using SMOO.Services.Interface;
using SMOO.Util;

namespace SMOO.Services.Impl;

internal class PlayerDisconnector : IPlayerDisconnector
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct PacketDisconnect
    {
        public required PacketHeader Header;
        public ushort SequenceNumber;

        public static int SizeOf()
        {
            return Unsafe.SizeOf<PacketDisconnect>();
        }

        public static ushort SizeOfPayload()
        {
            return sizeof(byte);
        }
    }

    public Result<Error> Disconnect(Player player)
    {
        RentedBuffer broadcastBuffer = MemoryUtil.Rent<PacketDisconnect>();

        broadcastBuffer.Write(new PacketDisconnect()
        {
            Header = new PacketHeader()
            {
                Type = PacketType.Disconnect,
                Flags = (byte)PacketFlags.None,
                Version = Config.Version,
                RoomId = player.Room.Id,
                PayloadSize = PacketDisconnect.SizeOfPayload()
            }
        });

        Result<Error> unregisterResult = player.Room.PlayerHolder.UnregisterPlayer(player);
        if (unregisterResult.IsFailed)
        {
            return unregisterResult;
        }

        return player.Room.Broadcaster.BroadcastAck(player.Room, new PacketBroadcastRequest()
        {
            MaxRetries = Config.MaxRetries,
            RentedPayload = broadcastBuffer
        });
    }
}
