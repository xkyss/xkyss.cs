<Query Kind="Program">
  <Namespace>System.Text.Json</Namespace>
</Query>

void Main()
{
	// Write code to test your extensions here. Press F5 to compile and run.
}

public static class JsonDumpExtensions
{
	public static string JsonPrettify(this string json)
	{
		using var jDoc = JsonDocument.Parse(json);
		return JsonSerializer.Serialize(jDoc, new JsonSerializerOptions { WriteIndented = true });
	}

	public static string ToJsonDump(this object data)
	{
		return JsonPrettify(JsonSerializer.Serialize(data)).Dump();
	}

	public static string DumpJson(this string code, string panelTitle = null)
	{
		return DumpWithSyntaxHightlighting(JsonPrettify(code), SyntaxLanguageStyle.Json, panelTitle);

	}

	public static string DumpCSharp(this string code, string panelTitle = null)
	{
		return DumpWithSyntaxHightlighting(code, SyntaxLanguageStyle.CSharp, panelTitle);
	}

	public static string DumpWithSyntaxHightlighting(this string code,
		SyntaxLanguageStyle language = SyntaxLanguageStyle.XML,
		string panelTitle = null)
	{
		if (panelTitle == null)
			panelTitle = Enum.GetName(typeof(SyntaxLanguageStyle), language) + "Result";

		PanelManager.DisplaySyntaxColoredText(code, language, panelTitle);
		return code;
	}
}

// You can also define namespaces, non-static classes, enums, etc.

#region Advanced - How to multi-target

// The NETx symbol is active when a query runs under .NET x or later.
// (LINQPad also recognizes NETx_0_OR_GREATER in case you enjoy typing.)

#if NET8
// Code that requires .NET 8 or later
#endif

#if NET7
// Code that requires .NET 7 or later
#endif

#if NET6
// Code that requires .NET 6 or later
#endif

#if NETCORE
// Code that requires .NET Core or later
#else
// Code that runs under .NET Framework (LINQPad 5)
#endif

#endregion