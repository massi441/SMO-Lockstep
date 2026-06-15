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
        public required byte PlayerSlot;

        public readonly void Serialize(ref SpanWriter writer)
        {
            writer.Write(this);
        }
    }

    public Result<Error> Disconnect(Player player)
    {
        Result<Error> unregisterResult = player.Room.PlayerHolder.UnregisterPlayer(player);
        if (unregisterResult.IsFailed)
        {
            return unregisterResult;
        }

        PacketDisconnect disconnectPacket = new PacketDisconnect()
        {
            Header = new PacketHeader()
            {
                Type = PacketType.Disconnect,
                Flags = (byte)PacketFlags.None,
                Version = Config.Version,
                RoomId = player.Room.Id,
            },
            PlayerSlot = player.Slot
        };

        RentedBuffer broadcastBuffer = PacketSerializer.Serialize(ref disconnectPacket, Unsafe.SizeOf<PacketDisconnect>());

        player.Room.Broadcaster.BroadcastReliably(player.Room.Players.Active, broadcastBuffer);

        return Result<Error>.Success();
    }
}
