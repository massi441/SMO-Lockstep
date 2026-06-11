using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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

    public void ReadInto<T>(ref T item) where T : struct
    {
        item = MemoryMarshal.Read<T>(RemainingSpan);
        _offset += Unsafe.SizeOf<T>();
    }

    public T Read<T>() where T : struct
    {
        T item = MemoryMarshal.Read<T>(RemainingSpan);
        _offset += Unsafe.SizeOf<T>();
        return item;
    }

    public byte ReadByte()
    {
        byte result = RemainingSpan[0];
        _offset += sizeof(byte);
        return result;
    }

    public sbyte ReadSByte()
    {
        sbyte result = (sbyte)RemainingSpan[0];
        _offset += sizeof(sbyte);
        return result;
    }

    public short ReadInt16LittleEndian()
    {
        short result = BinaryPrimitives.ReadInt16LittleEndian(RemainingSpan);
        _offset += sizeof(short);
        return result;
    }

    public ushort ReadUInt16LittleEndian()
    {
        ushort result = BinaryPrimitives.ReadUInt16LittleEndian(RemainingSpan);
        _offset += sizeof(ushort);
        return result;
    }

    public int ReadInt32LittleEndian()
    {
        int result = BinaryPrimitives.ReadInt32LittleEndian(RemainingSpan);
        _offset += sizeof(int);
        return result;
    }

    public uint ReadUInt32LittleEndian()
    {
        uint result = BinaryPrimitives.ReadUInt32LittleEndian(RemainingSpan);
        _offset += sizeof(uint);
        return result;
    }

    public long ReadInt64LittleEndian()
    {
        long result = BinaryPrimitives.ReadInt64LittleEndian(RemainingSpan);
        _offset += sizeof(long);
        return result;
    }

    public ulong ReadUInt64LittleEndian()
    {
        ulong result = BinaryPrimitives.ReadUInt64LittleEndian(RemainingSpan);
        _offset += sizeof(ulong);
        return result;
    }

    public float ReadSingleLittleEndian()
    {
        float result = BinaryPrimitives.ReadSingleLittleEndian(RemainingSpan);
        _offset += sizeof(float);
        return result;
    }

    public double ReadDoubleLittleEndian()
    {
        double result = BinaryPrimitives.ReadDoubleLittleEndian(RemainingSpan);
        _offset += sizeof(double);
        return result;
    }

    public ReadOnlySpan<byte> ReadBytes(int count)
    {
        ReadOnlySpan<byte> result = RemainingSpan[..count];
        _offset += count;
        return result;
    }

    public string ReadStringUTF8(int length)
    {
        return Encoding.UTF8.GetString(ReadBytes(length));
    }
}
