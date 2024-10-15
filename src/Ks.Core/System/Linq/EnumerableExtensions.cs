namespace System.Linq;

public static class EnumerableExtensions
{
    public static bool ContainsAll<TSource>(this IEnumerable<TSource> source, IEnumerable<TSource> values, IEqualityComparer<TSource>? comparer=null)
    {
        if (source == null || source.Count() == 0)
        {
            return false;
        }

        if (values == null || values.Count() == 0)
        {
            return false;
        }

        if (source.Count() < values.Count())
        {
            return false;
        }

        foreach (var value in values)
        {
            if (!source.Contains(value, comparer))
            {
                return false;
            }
        }

        return true;
    }
}
