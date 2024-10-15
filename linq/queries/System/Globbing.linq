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

	var result = matcher.Match("Globbing.linq");
	Assert.True(result.HasMatches);
}

/// <summary>任意文件名</summary>
[Fact]
public void Test02()
{
	var matcher = new Matcher();
	matcher.AddInclude("*.linq");

	var result = matcher.Match("Globbing.linq");
	Assert.True(result.HasMatches);
}

/// <summary>不带盘符会成功</summary>
[Fact]
public void Test03()
{

	var matcher = new Matcher();
	matcher.AddInclude("**/redis/*.conf");

	var result = matcher.Match("home/Ml-Cache/docker/redis/my.conf");
	Assert.True(result.HasMatches);
}

/// <summary>反斜杠可以成功</summary>
[Fact]
public void Test031()
{

	var matcher = new Matcher();
	matcher.AddInclude("**/redis/*.conf");

	var result = matcher.Match("home/Ml-Cache/docker/redis\\my.conf");
	Assert.True(result.HasMatches);
}

/// <summary>带盘符会失败</summary>
[Fact]
public void Test04()
{

	var matcher = new Matcher();
	matcher.AddInclude("**/redis/*.conf");

	var result = matcher.Match("D:/home/Ml-Cache/docker/redis/my.conf");
	Assert.False(result.HasMatches);
}

/// <summary>相对目录会失败</summary>
[Fact]
public void Test05()
{
	var matcher = new Matcher();
	matcher.AddInclude("**/redis/*.conf");

	var result = matcher.Match("../redis/my.conf");
	Assert.False(result.HasMatches);
}

/// <summary>测试真实目录</summary>
[Fact]
public void Test06()
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