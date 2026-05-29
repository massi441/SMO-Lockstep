using Lockstep.Protocol;

namespace Lockstep.Server;

internal class WorkItem
{
    public required byte[] RentedBuffer;
    public required Payload Packet;
}
