using System.Diagnostics.CodeAnalysis;

namespace Ks.Core;

public static partial class Check
{
    public static ICollection<T> NotNullOrEmpty<T>(ICollection<T> value, [NotNull] string parameterName)
    {
        if (value.IsNullOrEmpty())
        {
            throw new ArgumentException(parameterName + " can not be null or empty!", parameterName);
        }

        return value;
    }
}
