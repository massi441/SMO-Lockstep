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
    private struct PacketDisconnect : ISerializableStruct
    {
        public required PacketHeader Header;
        public ushort SequenceNumber;
        public required byte PlayerSlot;

        public static int SizeOf()
        {
            return Unsafe.SizeOf<PacketDisconnect>();
        }

        public static ushort SizeOfPayload()
        {
            return sizeof(byte);
        }

        public readonly void Serialize(Span<byte> destination)
        {
            MemoryMarshal.Write(destination, this);
        }
    }

    public Result<Error> Disconnect(Player player)
    {
        Result<Error> unregisterResult = player.Room.PlayerHolder.UnregisterPlayer(player);
        if (unregisterResult.IsFailed)
        {
            return unregisterResult;
        }

        RentedBuffer broadcastBuffer = MemoryUtil.Rent<PacketDisconnect>();
        PacketDisconnect disconnectPacket = new PacketDisconnect()
        {
            Header = new PacketHeader()
            {
                Type = PacketType.Disconnect,
                Flags = (byte)PacketFlags.None,
                Version = Config.Version,
                RoomId = player.Room.Id,
                PayloadSize = PacketDisconnect.SizeOfPayload()
            },
            PlayerSlot = player.Slot
        };

        PacketSerializer.Serialize(broadcastBuffer.UsedSpan, disconnectPacket);

        player.Room.Broadcaster.BroadcastReliably(player.Room, broadcastBuffer);

        return Result<Error>.Success();
    }
}
