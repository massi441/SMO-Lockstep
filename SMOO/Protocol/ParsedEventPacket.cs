using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SMOO.Event;

namespace SMOO.Protocol;

internal readonly struct ParsedEventPacket
{
    public required ParsedPacket BasePacket { get; init; }

    public ref EventHeader EventHeader => ref MemoryMarshal.AsRef<EventHeader>(BasePacket.Payload);

    public Span<byte> EventData => BasePacket.Payload[Unsafe.SizeOf<EventHeader>()..];
}
