using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Lockstep.Util;

internal ref struct SpanWriter
{
    private int _offset;

    public Span<byte> Span { get; }

    public SpanWriter(Span<byte> span)
    {
        Span = span;
    }

    public void Reset()
    {
        _offset = 0;
    }

    public void Write<T>(T value) where T : struct
    {
        MemoryMarshal.Write(Span[_offset..], value);
        _offset += Unsafe.SizeOf<T>();
    }
}
