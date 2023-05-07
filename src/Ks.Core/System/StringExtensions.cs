using System.Numerics;

namespace System;

public static class StringConvertExtensions
{
    public static string[] SplitByLength(this string @this, int length)
    {
        if (string.IsNullOrEmpty(@this))
        {
            return new string[0];
        }

		// 总份数
		int count = @this.Length / length;
		// 最后一部分的长度
		int lastLength = @this.Length % length;

        // 预定结果
		string[] result = new string[count + (lastLength > 0 ? 1 : 0)];

        // 前面的部分
		for (int i = 0; i < count; i++)
		{
			result[i] = @this.Substring(i * length, length);
		}
        // 最后一部分
		if (lastLength > 0)
		{
			result[count] = @this.Substring(count * length, lastLength);
		}

        return result;
	}

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

    public static bool IsAllUpper(this string @this)
    {
        if (string.IsNullOrEmpty(@this))
        {
            return false;
        }

		return @this.All(char.IsUpper);
	}

	public static bool IsAllLower(this string @this)
	{
		if (string.IsNullOrEmpty(@this))
		{
			return false;
		}

		return @this.All(char.IsLower);
	}
}
