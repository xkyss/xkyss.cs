<Query Kind="Statements" />

// 遍历目录

const string dir = @"D:\Code\thzt\mlcache-doc\deploy\192.168.1.144";

EnumerateFilesRecursively(new DirectoryInfo(dir));

static void EnumerateFilesRecursively(DirectoryInfo di)
{
	try
	{
		foreach (FileInfo file in di.EnumerateFiles())
		{
			Console.Write(file.FullName);
			Console.Write("  ");
			Console.WriteLine(Path.GetRelativePath(dir, file.Directory.FullName));
		}

		foreach (DirectoryInfo subDir in di.EnumerateDirectories())
		{
			Console.Write(subDir.FullName);
			Console.Write("  ");
			Console.WriteLine(Path.GetRelativePath(subDir.FullName, dir));
			EnumerateFilesRecursively(subDir);
		}
	}
	catch (Exception ex)
	{
		Console.WriteLine("An error occurred: " + ex.Message);
	}
}