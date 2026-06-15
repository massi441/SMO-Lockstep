namespace SMOO.Util;

internal interface ISerializableStruct
{
    void Serialize(ref SpanWriter writer);
}
