using System.Net;
using Lockstep.Util;

namespace Lockstep.Protocol;

internal readonly struct Packet
{
    public readonly IPEndPoint Sender;
    public readonly RentedBuffer<byte> RentedBuffer;

    public Packet(IPEndPoint sender, RentedBuffer<byte> buffer)
    {
        RentedBuffer = buffer;
        Sender = sender;
    }
}
