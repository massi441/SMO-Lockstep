using System.Numerics;
using System.Runtime.InteropServices;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Util;

namespace SMOO.Event;

internal class EventPlayerSyncHandler : IEventHandler
{
    // TODO: Add [Required] attribute on struct, to deduce minimum payload size
    public static ushort MinPayloadSize => RequiredSize<PlayerSyncData>.Size;

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct PlayerSyncData
    {
        [RequiredField] public Vector3 Position;
        [RequiredField] public Quaternion Quat;
    }

    public static void Handle(ParsedEventPacket eventPacket, Room room, ServerContext context)
    {
        room.Broadcaster.BroadcastExcept(room, eventPacket.BasePacket.SenderPlayer!, eventPacket.BasePacket.RentedBuffer.UsedSpan);

        eventPacket.BasePacket.RentedBuffer.Return();
    }
}
