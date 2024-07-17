<Query Kind="Program">
  <NuGetReference>Microsoft.Extensions.FileSystemGlobbing</NuGetReference>
  <Namespace>Microsoft.Extensions.FileSystemGlobbing</Namespace>
  <Namespace>Xunit</Namespace>
</Query>

// https://learn.microsoft.com/zh-cn/dotnet/core/extensions/file-globbing

#load "xunit"

void Main()
{
	RunTests();
}



/// <summary>任意目录 + 任意文件名</summary>
[Fact]
public void Test01()
{
	var matcher = new Matcher();
	matcher.AddInclude("**/*.linq");

	PatternMatchingResult result = matcher.Match("Globbing.linq");
	Assert.True(result.HasMatches);
}

/// <summary>任意文件名</summary>
[Fact]
public void Test02()
{
	var matcher = new Matcher();
	matcher.AddInclude("*.linq");

	PatternMatchingResult result = matcher.Match("Globbing.linq");
	Assert.True(result.HasMatches);
}

/// <summary>测试真实目录</summary>
[Fact]
public void Test03()
{
	var matcher = new Matcher();
	matcher.AddIncludePatterns(new[] { "**/*.txt", "**/*.linq" });

	var p = Path.Combine(LINQPad.Util.MyQueriesFolder, "System");

	var files = matcher.GetResultsInFullPath(p);
	Assert.True(files.Count() > 0);
	
	//foreach (string file in files)
	//{
	//	Console.WriteLine(file);
	//}
}