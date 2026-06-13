using System.Reflection;
using System.Runtime.InteropServices;

namespace SMOO.Util;

internal static class RequiredSize<T> where T : struct
{
    public static readonly ushort Size = Compute(typeof(T));

    public static ushort Compute(Type type)
    {
        ushort requiredSize = 0;

        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        bool hasRequiredFields = fields.Any(f => f.IsDefined(typeof(RequiredFieldAttribute)));
        if (!hasRequiredFields)
        {
            return (ushort)Marshal.SizeOf(type);
        }

        foreach (FieldInfo field in fields)
        {
            bool isRequired = field.IsDefined(typeof(RequiredFieldAttribute), inherit: false);

            if (!isRequired)
            {
                continue;
            }

            requiredSize += Compute(field.FieldType);

        }

        return requiredSize;
    }
}
