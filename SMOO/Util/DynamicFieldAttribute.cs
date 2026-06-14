namespace SMOO.Util;

[AttributeUsage(AttributeTargets.Field)]
internal class DynamicFieldAttribute : Attribute
{
    public required ushort MaxSize { get; init; }
}
