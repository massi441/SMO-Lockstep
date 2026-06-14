using System.Numerics;
using System.Runtime.InteropServices;
using SMOO.Client;
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
        PlayerSyncData playerSyncData = PacketSerializer.Deserialize<PlayerSyncData>(eventPacket.EventData);

        Player[] playersInStage = room.PlayerHolder.InSameStageAs(eventPacket.BasePacket.SenderPlayer!);

        room.Broadcaster.Broadcast(playersInStage, eventPacket.BasePacket.RentedBuffer);

        eventPacket.BasePacket.RentedBuffer.Return();
    }
}
