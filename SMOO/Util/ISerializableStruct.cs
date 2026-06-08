namespace SMOO.Util;

internal interface ISerializableStruct
{
    void Serialize(Span<byte> destination);
}
