using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Util;

namespace SMOO.Event;

internal class EventPlayerSyncHandler : IEventHandler
{
    public static ushort MinPayloadSize => (ushort)Unsafe.SizeOf<PlayerSyncData>();

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct PlayerSyncData : IDeserializableStruct
    {
        public Vector3 Position;
        public Quaternion Quaternion;

        public void Deserialize(ref SpanReader reader)
        {
            reader.ReadInto(ref this);
        }
    }

    public static void Handle(ParsedEventPacket eventPacket, Room room, ServerContext context)
    {
        room.Broadcaster.BroadcastExcept(room, eventPacket.BasePacket.SenderPlayer!, eventPacket.BasePacket.RentedBuffer.UsedSpan);

        eventPacket.BasePacket.RentedBuffer.Return();
    }
}
