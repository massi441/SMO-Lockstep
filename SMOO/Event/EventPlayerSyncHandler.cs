using System.Numerics;
using System.Runtime.InteropServices;
using SMOO.Client;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Util;

namespace SMOO.Event;

internal class EventPlayerSyncHandler : IEventHandler
{
    public static ushort MinPayloadSize => RequiredSize<PlayerSyncData>.MinSize;  

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private ref struct PlayerSyncData
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
    }

    public static void Handle(ParsedEventPacket eventPacket, Room room, ServerContext context)
    {
        Player[] playersInStage = room.PlayerHolder.InSameStageAs(eventPacket.BasePacket.SenderPlayer!);

        room.Broadcaster.Broadcast(playersInStage, eventPacket.BasePacket.RentedBuffer.UsedSpan);

        eventPacket.BasePacket.RentedBuffer.Return();
    }
}
