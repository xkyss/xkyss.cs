<Query Kind="Statements" />


// 指定目录, 把所有文件重命名为 YYYY.mm.dd-HHMMSS.ext

var dir = @"D:\Material\material.xkyii.com\home\2024\电位";
var di = new DirectoryInfo(dir);

foreach (FileInfo file in di.EnumerateFiles())
{
	var newFileName = $"{file.CreationTime:yyyy.MM.dd-HHmmss}.{file.Extension}";

	var newFilePath = Path.Combine(dir, newFileName);
	// 检查新文件名是否已经存在，如果不存在，则重命名
	if (!File.Exists(newFilePath))
	{
		File.Move(file.FullName, newFilePath);
		Console.WriteLine($"{file.Name} -> {newFileName}");
	}
	else
	{
		Console.WriteLine($"{file.Name} 已存在");
	}
}