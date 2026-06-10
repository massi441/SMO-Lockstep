using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace SMOO.Util;

internal ref struct SpanWriter
{
    private readonly Span<byte> _span;
    private int _offset;

    public readonly int Offset => _offset;

    /// <summary>
    /// Returns a span starting at the current offset of the writer
    /// </summary>
    public readonly Span<byte> RemainingSpan => _span[_offset..];

    public SpanWriter(Span<byte> span)
    {
        _span = span;
    }

    public void Reset()
    {
        _offset = 0;
    }

    public void Skip(int offset)
    {
        _offset += offset;
    }

    public void Write<T>(T value) where T : struct
    {
        MemoryMarshal.Write(RemainingSpan, value);
        _offset += Unsafe.SizeOf<T>();
    }

    public void WriteString(string str)
    {
        Encoding.UTF8.GetBytes(str, RemainingSpan);
        _offset += Encoding.UTF8.GetByteCount(str);
    }
}
