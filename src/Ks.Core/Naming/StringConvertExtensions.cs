using System.Text.RegularExpressions;

namespace Ks.Core.Naming;

public static class StringConvertExtensions
{
	private static readonly string[] EMPTY_STRING_ARRAY = new string[0];

	/// <summary>
	/// 自动切分, 尝试按'_', '-', ',', ' '来切分,如果都不符合就按驼峰方式分割
	/// </summary>
	/// <param name="this"></param>
	/// <returns></returns>
	public static string[] SplitAuto(this string @this)
	{
		if (string.IsNullOrEmpty(@this))
		{
			return EMPTY_STRING_ARRAY;
		}

		foreach (char c in new [] { '_', '-', ',', ' ' })
		{
			if (@this.Contains(c))
			{
				return @this.Split(c, StringSplitOptions.RemoveEmptyEntries);
			}
		}

		return @this.SplitCamel();
	}

	/// <summary>
	/// 按驼峰方式分割字符串,遇到大写字母就进行分割
	/// </summary>
	public static string[] SplitCamel(this string @this)
	{
		var pattern = @"(?<!^)(?=[A-Z])";
		var result = Regex.Split(@this, pattern);
		return result;
	}

	/// <summary>
	/// 首字母大写, {code '_'}连接
	/// 例: Ada_Case_Name
	/// </summary>
	public static string ToAda(this IEnumerable<string> @this)
	{
		var words = @this.Select(FirstWordUpper);
		return string.Join("_", words);
	}

	/// <summary>
	/// 第一个单次首字母小写, 其他首字母大写
	/// 例: camelCaseName
	/// </summary>
	public static string ToCamel(this IEnumerable<string> @this)
	{
		var words = @this.Select((word, i) => (i==0) ? word.ToLower() : FirstWordUpper(word));
		return string.Concat(words);
	}

	/// <summary>
	/// 全大写, {code '-'}连接
	/// 例: COBEL-CASE-NAME
	/// </summary>
	public static string ToCobol(this IEnumerable<string> @this)
	{
		var words = @this.Select(word => word.ToUpper());
		return string.Join("-", words);
	}

	/// <summary>
	/// 全小写, {code '_'}连接
	/// 例: lower_case_name
	/// </summary>
	public static string ToMacro(this IEnumerable<string> @this)
	{
		var words = @this.Select(word => word.ToLower());
		return string.Join("_", words);
	}

	/// <summary>
	/// 全小写, {code '.'}连接
	/// 例: dot.case.name
	/// </summary>
	public static string ToDot(this IEnumerable<string> @this)
	{
		var words = @this.Select(word => word.ToLower());
		return string.Join(".", words);
	}

	/// <summary>
	/// 全小写, {code '-'}连接
	/// 例: kebab-case-name
	/// </summary>
	public static string ToKebab(this IEnumerable<string> @this)
	{
		var words = @this.Select(word => word.ToLower());
		return string.Join(".", words);
	}

	/// <summary>
	/// 全小写, {code ' '}连接
	/// 例: lower case name
	/// </summary>
	public static string ToLower(this IEnumerable<string> @this)
	{
		var words = @this.Select(word => word.ToLower());
		return string.Join(" ", words);
	}

	/// <summary>
	/// 首字母大写
	/// 例: CamelCaseName
	/// </summary>
	public static string ToPascal(this IEnumerable<string> @this)
	{
		var words = @this.Select(FirstWordUpper);
		return string.Concat(words);
	}

	/// <summary>
	/// 第一个单词首字母大写, 其余单词小写, ' '连接
	/// 例: Sentence case name
	/// </summary>
	public static string ToSentence(this IEnumerable<string> @this)
	{
		var words = @this.Select((word, i) => (i == 0) ? FirstWordUpper(word) : word.ToLower());
		return string.Join(" ", words);
	}

	/// <summary>
	/// 全小写, '_'连接
	/// 例: snake_case_name
	/// </summary>
	public static string ToSnake(this IEnumerable<string> @this)
	{
		var words = @this.Select(word => word.ToLower());
		return string.Join("_", words);
	}

	/// <summary>
	/// 全部单词首字母大写, {@code ' '}连接
	/// 例: Title Case Name
	/// </summary>
	public static string ToTitle(this IEnumerable<string> @this)
	{
		var words = @this.Select(FirstWordUpper);
		return string.Join(" ", words);
	}

	/// <summary>
	/// 全部单词首字母大写, {@code '-'}连接
	/// 例: Title-Case-Name
	/// </summary>
	public static string ToTrain(this IEnumerable<string> @this)
	{
		var words = @this.Select(FirstWordUpper);
		return string.Join("-", words);
	}

	/// <summary>
	/// 全大写, {@code ' '}连接
	/// 例: UPPER CASE NAME
	/// </summary>
	public static string ToUpper(this IEnumerable<string> @this)
	{
		var words = @this.Select(word => word.ToUpper());
		return string.Join(" ", words);
	}

	/// <summary>
	/// 将字符串转换为首字母大写,其他字母小写
	/// 如: aaaa -> Aaaa
	/// </summary>
	public static string FirstWordUpper(this string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return s;
		}

		return char.ToUpper(s[0]) + s.Substring(1).ToLower();
	}
}
