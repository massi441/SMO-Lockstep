using System.Net;

namespace Lockstep.Protocol;

internal readonly struct Payload
{
    public readonly Memory<byte> Buffer;
    public readonly IPEndPoint Sender;

    public readonly ReadOnlySpan<byte> Span => Buffer.Span;

    public readonly int Length => Buffer.Length;

    public Payload(Memory<byte> payload, IPEndPoint sender)
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
