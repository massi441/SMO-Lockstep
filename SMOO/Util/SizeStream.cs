using System.Runtime.CompilerServices;
using System.Text;

namespace SMOO.Util;

internal struct SizeStream
{
    private ushort _size;
    public readonly ushort Size => _size;

    public void Write<T>() where T : unmanaged
    {
        _size += (ushort)Unsafe.SizeOf<T>();
    }

    public void WriteTimes<T>(ushort times) where T : unmanaged
    {
        _size += (ushort)(Unsafe.SizeOf<T>() * times);
    }

    public void WriteBytes(ushort byteCount)
    {
        _size += byteCount;
    }

    public void WriteString(string str)
    {
        _size += (ushort)Encoding.UTF8.GetByteCount(str.AsSpan());
    }
}
