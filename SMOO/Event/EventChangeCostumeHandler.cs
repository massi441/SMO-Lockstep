using SMOO.Protocol;
using SMOO.Server;
using SMOO.Util;

namespace SMOO.Event;

internal class EventChangeCostumeHandler : IEventHandler
{
    public static ushort MinDataSize => 0;
    public static ushort MaxDataSize => Config.MaxBufferSize;

    public static void Handle(ParsedEventPacket packet, Room room, ServerContext context)
    {
        packet.BasePacket.RentedBuffer.Return();
    }
}
