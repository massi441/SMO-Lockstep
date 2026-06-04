namespace SMOO.Protocol;

internal interface ISerializableStruct
{
    void Serialize(Span<byte> destination);
}
