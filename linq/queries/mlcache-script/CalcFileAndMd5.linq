<Query Kind="Statements">
  <NuGetReference>Microsoft.Extensions.FileSystemGlobbing</NuGetReference>
  <Namespace>Microsoft.Extensions.FileSystemGlobbing</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>



var path = @"D:\Code\thzt\count";
var csv = Path.Combine(path, "file-md5.csv");
var ignore = new Matcher();


ignore.AddIncludePatterns(new[] {"./*", "**/.git/**/*", "**/.gitignore", "**/README*", "mlcache-web/public/pdf/**/*"});

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
using var writer = new StreamWriter(csv, false, System.Text.Encoding.GetEncoding("GB2312"));

var filenames = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
Console.WriteLine($"文件总数: {filenames.Count()}");

var ignoreNum = 0;
var index = 0;
writer.WriteLine("序号,源文件路径及文件名,文件MD5");
foreach (var filename in filenames)
{
	//file.Dump();
	var relpath = Path.GetRelativePath(path, filename);

	if (ignore.Match(relpath).HasMatches)
	{
		ignoreNum++;
		continue;
	}

	var md5 = CalculateMD5(filename);
	writer.WriteLine($"{++index},{relpath},{md5}");
}
Console.WriteLine($"忽略总数: {ignoreNum}");
Console.WriteLine($"处理总数: {index}");



static string CalculateMD5(string filePath)
{
	using var md5 = MD5.Create();
	using var stream = File.OpenRead(filePath);
	var hash = md5.ComputeHash(stream);
	return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
}
