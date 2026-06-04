using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SMOO.Util;

internal ref struct SpanWriter
{
    private readonly Span<byte> _span;
    private int _offset;

    public readonly int Offset => _offset;

    /// <summary>
    /// Returns a span starting at the current offset of the writer
    /// </summary>
    public readonly Span<byte> CurrentSpan => _span[_offset..];

    public SpanWriter(Span<byte> span)
    {
        _span = span;
    }

    public void Reset()
    {
        _offset = 0;
    }

    public void Jump(int offset)
    {
        _offset += offset;
    }

    public void Write<T>(T value) where T : struct
    {
        MemoryMarshal.Write(CurrentSpan, value);
        _offset += Unsafe.SizeOf<T>();
    }
}
