<Query Kind="Statements">
  <NuGetReference>RazorEngineCore</NuGetReference>
  <Namespace>RazorEngineCore</Namespace>
</Query>

// 遍历模板目录, 生成到指定目录 (保持目录结构一致)

var templatePath = @"D:\Code\thzt\mlcache-doc\deploy\generating";
var targetPath = @"D:\Code\thzt\mlcache-doc\deploy\generated";


var templates = EnumerateFilesRecursively(new DirectoryInfo(templatePath));
//templates.Dump();

var razorEngine = new RazorEngine();
var model = new
{
	Username = "appstore",
	HostIp = "192.168.1.1442",
	WorkDir = "/home/Ml-Cache",
};

foreach (var t in templates)
{
	var templateContent = File.ReadAllText(t.FullName);
	var template = razorEngine.Compile(templateContent);
	var result = template.Run(model);

	// 构造目标文件的路径
	string relativePath = Path.GetRelativePath(templatePath, t.Directory.FullName);
	//relativePath.Dump();
	string targetFile = Path.Combine(targetPath, relativePath, t.Name);
	//targetFile.Dump();
	//File.WriteAllText(result);
	// 确保目标文件的目录存在
	string targetDirectory = Path.GetDirectoryName(targetFile);
	if (!Directory.Exists(targetDirectory))
	{
		Directory.CreateDirectory(targetDirectory);
	}
	File.WriteAllText(targetFile, result);
}


static List<FileInfo> EnumerateFilesRecursively(DirectoryInfo di)
{
	var list = new List<FileInfo>();
	try
	{
		foreach (FileInfo file in di.EnumerateFiles())
		{
			if (file.Extension.EndsWith(".linq")) {
				continue;
			}
			//Console.Write(file.FullName);
			//Console.Write("  ");
			//Console.WriteLine(Path.GetRelativePath(file.Directory.FullName, di.FullName));
			list.Add(file);
		}

		foreach (DirectoryInfo subDir in di.EnumerateDirectories())
		{
			//Console.Write(subDir.FullName);
			//Console.Write("  ");
			//Console.WriteLine(Path.GetRelativePath(subDir.FullName, di.FullName));
			list.AddRange(EnumerateFilesRecursively(subDir));
		}
		return list;
	}
	catch (Exception ex)
	{
		Console.WriteLine("An error occurred: " + ex.Message);
		return new List<FileInfo>();
	}
}