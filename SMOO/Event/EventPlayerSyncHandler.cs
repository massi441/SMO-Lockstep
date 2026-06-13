using System.Numerics;
using System.Runtime.InteropServices;
using SMOO.Client;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Util;

namespace SMOO.Event;

internal class EventPlayerSyncHandler : IEventHandler
{
    // TODO: Add [Required] attribute on struct, to deduce minimum payload size
    public static ushort MinPayloadSize => RequiredSize<PlayerSyncData>.Size;  

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private ref struct PlayerSyncData
    {
        [RequiredField] public Vector3 Position;
        [RequiredField] public Quaternion Quat;
        [RequiredField] public float AnimRate;
        [RequiredField] public StreamStringView<byte> Anim;
        [RequiredField] public StreamStringView<byte> SubAnim;
        [RequiredField] public StreamStringView<byte> UpperAnim;
        [RequiredField] public StreamSpanView<byte, float> BlendWeights;
    }

    public static void Handle(ParsedEventPacket eventPacket, Room room, ServerContext context)
    {
        Player[] playersInStage = room.PlayerHolder.InSameStageAs(eventPacket.BasePacket.SenderPlayer!);

        room.Broadcaster.BroadcastExcept(playersInStage, eventPacket.BasePacket.SenderPlayer!, eventPacket.BasePacket.RentedBuffer.UsedSpan);

        eventPacket.BasePacket.RentedBuffer.Return();
    }
}
