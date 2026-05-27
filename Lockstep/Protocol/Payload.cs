using System.Net;

namespace Lockstep.Protocol;

internal readonly ref struct Payload
{
    public readonly ReadOnlySpan<byte> Buffer;
    public readonly EndPoint Sender;

    public Payload(ReadOnlySpan<byte> payload, EndPoint sender)
    {
        Buffer = payload;
        Sender = sender;
    }

    public Payload(Payload other, int startOffset)
    {
        Buffer = other.Buffer[startOffset..];
        Sender = other.Sender;
    }
}
