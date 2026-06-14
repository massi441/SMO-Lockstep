using System.Numerics;
using System.Runtime.InteropServices;
using SMOO.Enumerator;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Util;

namespace SMOO.Event;

internal class EventPlayerSyncHandler : IEventHandler
{
    public static ushort MinDataSize => RequiredSize<PlayerSyncData>.MinSize;
    public static ushort MaxDataSize => RequiredSize<PlayerSyncData>.MaxSize;  

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private ref struct PlayerSyncData : IDeserializableStruct
    {
        [RequiredField]
        public Vector3 Position;

        [RequiredField]
        public Quaternion Quat;

        [RequiredField]
        public float AnimRate;

        [DynamicField(MaxSize = Config.MaxAnimNameLength)]
        public StreamStringView<byte> Anim;
        
        [DynamicField(MaxSize = Config.MaxAnimNameLength)]
        public StreamStringView<byte> SubAnim;
        
        [DynamicField(MaxSize = Config.MaxAnimNameLength)]
        public StreamStringView<byte> UpperAnim;
        
        [DynamicField(MaxSize = Config.MaxBlendWeights * sizeof(float))]
        public StreamSpanView<byte, float> BlendWeights;

        public void Deserialize(ref SpanReader reader)
        {
            reader.ReadInto(ref Position);
            reader.ReadInto(ref Quat);

            AnimRate = reader.ReadSingleLittleEndian();

            Anim.Deserialize(ref reader, Config.MaxAnimNameLength);
            SubAnim.Deserialize(ref reader, Config.MaxAnimNameLength);
            UpperAnim.Deserialize(ref reader, Config.MaxAnimNameLength);
            BlendWeights.Deserialize(ref reader, Config.MaxBlendWeights);
        }
    }

    public static void Handle(ParsedEventPacket eventPacket, Room room, ServerContext context)
    {
        PacketSerializer.Deserialize<PlayerSyncData>(eventPacket.EventData); // for validation only

        PlayerSameStageEnumerator enumerator = room.PlayerHolder.Players.SameStageAs(eventPacket.BasePacket.SenderPlayer!);
        room.Broadcaster.Broadcast(enumerator, eventPacket.BasePacket.RentedBuffer);

        eventPacket.BasePacket.RentedBuffer.Return();
    }
}
