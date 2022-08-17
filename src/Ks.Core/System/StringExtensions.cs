using System.Numerics;

namespace System;

public static class StringExtensions
{
    public static string SplitAndTakeLast(this string @this, string separator = "=")
    {
        if (string.IsNullOrEmpty(@this))
        {
            return @this;
        }

        var list = @this.Split(separator);
        // 没有正确的切分
        if (list.Length < 2)
        {
            return string.Empty;
        }

        return list[list.Length - 1];
    }

    public static string DivideBy(this string @this, int count)
    {
        if (string.IsNullOrEmpty(@this))
        {
            return @this;
        }

        if (count == 0)
        {
            return float.NaN.ToString();
        }

        if (!BigInteger.TryParse(@this, out var number))
        {
            return float.NaN.ToString();
        }

        var ret = number / count;
        return ret.ToString();
    }
}
