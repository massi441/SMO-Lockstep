using System.Net;

namespace Lockstep.Protocol;

internal readonly ref struct Packet
{
    public readonly ReadOnlySpan<byte> Buffer;
    public readonly EndPoint Sender;

    public Packet(ReadOnlySpan<byte> rawBuffer, int bytesReceived, EndPoint sender)
    {
        Buffer = rawBuffer[..bytesReceived];
        Sender = sender;
    }
}
