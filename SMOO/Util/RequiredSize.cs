using System.Reflection;
using System.Runtime.InteropServices;

namespace SMOO.Util;

internal static class RequiredSize<T> where T : struct, allows ref struct
{
    public static readonly ushort MinSize;
    public static readonly ushort MaxSize;

    static RequiredSize()
    {
        (MinSize, MaxSize) = Compute(typeof(T));
    }

    private static (ushort Min, ushort Max) Compute(Type type)
    {
        ushort minSize = 0;
        ushort maxSize = 0;

        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        bool hasSizeFields = fields.Any(f => f.IsDefined(typeof(RequiredFieldAttribute)) || f.IsDefined(typeof(DynamicFieldAttribute)));
        if (!hasSizeFields)
        {
            minSize += (ushort)Marshal.SizeOf(type);
            return (minSize, minSize);
        }

        foreach (FieldInfo field in fields)
        {
            if (field.IsDefined(typeof(RequiredFieldAttribute)))
            {
                (ushort Min, ushort Max) = Compute(field.FieldType);
                minSize += Min;
                maxSize += Max;
            }
            else if (field.IsDefined(typeof(DynamicFieldAttribute)))
            {
                DynamicFieldAttribute attribute = field.GetCustomAttribute<DynamicFieldAttribute>()!;

                (ushort Min, ushort _) = Compute(field.FieldType);

                minSize += Min;
                maxSize += (ushort)(Min + attribute.MaxSize);
            }
        }

        return (minSize, maxSize);
    }
}
