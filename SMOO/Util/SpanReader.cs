using System.Buffers.Binary;
using System.Text;

namespace SMOO.Util;

internal ref struct SpanReader
{
    private int _offset;
    private readonly ReadOnlySpan<byte> _span;

    public readonly int Remaining => _span.Length - _offset;
    public readonly ReadOnlySpan<byte> RemainingSpan => _span[_offset..];

    public SpanReader(ReadOnlySpan<byte> span)
    {
        _offset = 0;
        _span = span;
    }

    public SpanReader(Memory<byte> memory) : this(memory.Span)
    {

    }

    public void Reset()
    {
        _offset = 0;
    }

    public void Skip(int bytes)
    {
        _offset += bytes;
    }

    public byte ReadByte()
    {
        byte result = _span[_offset];
        _offset += sizeof(byte);
        return result;
    }

    public sbyte ReadSByte()
    {
        sbyte result = (sbyte)_span[_offset];
        _offset += sizeof(sbyte);
        return result;
    }

    public short ReadInt16LittleEndian()
    {
        short result = BinaryPrimitives.ReadInt16LittleEndian(_span.Slice(_offset, sizeof(short)));
        _offset += sizeof(short);
        return result;
    }

    public ushort ReadUInt16LittleEndian()
    {
        ushort result = BinaryPrimitives.ReadUInt16LittleEndian(_span.Slice(_offset, sizeof(ushort)));
        _offset += sizeof(ushort);
        return result;
    }

    public int ReadInt32LittleEndian()
    {
        int result = BinaryPrimitives.ReadInt32LittleEndian(_span.Slice(_offset, sizeof(int)));
        _offset += sizeof(int);
        return result;
    }

    public uint ReadUInt32LittleEndian()
    {
        uint result = BinaryPrimitives.ReadUInt32LittleEndian(_span.Slice(_offset, sizeof(uint)));
        _offset += sizeof(uint);
        return result;
    }

    public long ReadInt64LittleEndian()
    {
        long result = BinaryPrimitives.ReadInt64LittleEndian(_span.Slice(_offset, sizeof(long)));
        _offset += sizeof(long);
        return result;
    }

    public ulong ReadUInt64LittleEndian()
    {
        ulong result = BinaryPrimitives.ReadUInt64LittleEndian(_span.Slice(_offset, sizeof(ulong)));
        _offset += sizeof(ulong);
        return result;
    }

    public float ReadSingleLittleEndian()
    {
        float result = BinaryPrimitives.ReadSingleLittleEndian(_span.Slice(_offset, sizeof(float)));
        _offset += sizeof(float);
        return result;
    }

    public double ReadDoubleLittleEndian()
    {
        double result = BinaryPrimitives.ReadDoubleLittleEndian(_span.Slice(_offset, sizeof(double)));
        _offset += sizeof(double);
        return result;
    }

    public ReadOnlySpan<byte> ReadBytes(int count)
    {
        ReadOnlySpan<byte> result = _span.Slice(_offset, count);
        _offset += count;
        return result;
    }

    public string ReadStringUTF8(int length)
    {
        return Encoding.UTF8.GetString(ReadBytes(length));
    }
}
