using System.Numerics;
using System.Runtime.InteropServices;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Util;

namespace SMOO.Event;

internal class EventPlayerSyncHandler : IEventHandler
{
    public static ushort MinPayloadSize => throw new NotImplementedException();

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

    public static void Handle(ParsedPacket packet, Room room, ServerContext context, ReadOnlySpan<byte> eventData)
    {
        
    }
}
