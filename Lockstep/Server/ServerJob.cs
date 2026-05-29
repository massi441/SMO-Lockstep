using Lockstep.Protocol;

namespace Lockstep.Server;

internal class ServerJob
{
    public required byte[] RentedBuffer;
    public required Payload Packet;
}
