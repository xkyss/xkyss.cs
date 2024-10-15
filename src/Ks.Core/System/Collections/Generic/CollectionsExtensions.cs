namespace System.Collections.Generic;

public static class CollectionsExtensions
{
    public static bool IsNullOrEmpty<T>(this ICollection<T> source)
    {
        return source == null || source.Count <= 0;
    }

}
