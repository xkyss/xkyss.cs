<Query Kind="Program">
  <NuGetReference>NPOI</NuGetReference>
  <Namespace>NPOI.XSSF.UserModel</Namespace>
  <Namespace>NPOI.SS.UserModel</Namespace>
  <Namespace>NPOI.HSSF.Util</Namespace>
</Query>

void Main()
{
	var path1 = @"D:\Code\thzt\mlcache-doc\doc\97.draft\20241204\zsb1.xlsx";
	var path2 = @"D:\Code\thzt\mlcache-doc\doc\97.draft\20241204\zsb2.xlsx";
	var path3 = @"D:\Code\thzt\mlcache-doc\doc\97.draft\20241204\zsb3.xlsx";
	var map1 = GetMap1(path1);
	var map2 = GetMap2(path2);
	Fill(map1, map2);
	Write(map2, path3);
}

// You can define other methods, fields, classes and namespaces here

/// <summary>获取正向追溯表的内容(全)</summary>
static Dictionary<string, HashSet<string>> GetMap1(string path)
{
	using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);

	// 创建工作簿 (.xlsx)
	var workbook = new XSSFWorkbook(fs);
	// 获取工作表
	var sheet = workbook.GetSheetAt(0);

	// 存放结果
	var map = new Dictionary<string, HashSet<string>>();
	// 上一个key
	var last = (KeyValuePair<string, HashSet<string>>?) null;
	// 遍历工作表中的每一行
	for (int rowIdx = 0; rowIdx <= sheet.LastRowNum; rowIdx++)
	{
		// 获取当前行
		var row = sheet.GetRow(rowIdx);
		// 确保行不为空
		if (row == null)
		{
			continue;
		}

		// 表头
		if (rowIdx == 0)
		{
			//Console.WriteLine(stringOf(row));
		}
		else
		{
			var data = GetData(row);
			if (data == null)
			{
				continue;
			}
			// 新
			if (!string.IsNullOrEmpty(data?.Key))
			{
				last = data;
				map.Add(last?.Key, last?.Value);
				//Console.WriteLine(last?.Key);
			}
			// 旧,合并
			else
			{
				last?.Value.UnionWith(data?.Value);
			}
		}
	}

	Console.WriteLine($"总数(表1): {map.Count}");

		//// 打印一下
		//foreach (var r in map)
		//{
		//	var x = r.Key;
		//	var y = string.Join("\n", r.Value);
		//	Console.WriteLine($"{x}: {y}");
		//}	
	return map;
}

/// <summary>获取逆向追溯表的内容(内容待填充)</summary>
static Dictionary<string, HashSet<string>> GetMap2(string path)
{
	using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);

	// 创建工作簿 (.xlsx)
	var workbook = new XSSFWorkbook(fs);
	// 获取工作表
	var sheet = workbook.GetSheetAt(0);

	var result = new Dictionary<string, HashSet<string>>();
	result.Add("-", new HashSet<string>());
	// 遍历工作表中的每一行
	for (int rowIdx = 0; rowIdx <= sheet.LastRowNum; rowIdx++)
	{
		// 获取当前行
		var row = sheet.GetRow(rowIdx);
		// 确保行不为空
		if (row == null)
		{
			continue;
		}

		// 表头
		if (rowIdx < 3)
		{
			continue;
			//Console.WriteLine(stringOf(row));
		}
		//Console.WriteLine(stringOf(row));
		
		var c1 = row.GetCell(1);
		if (c1 == null || c1.ToString().Trim().Length == 0)
		{
			Console.WriteLine($"row {rowIdx} FAILED");
			continue;
		}
		
		result.Add(c1.ToString().Trim(), new HashSet<string>());
	}
	//Console.WriteLine($"总数(表2): {result.Count}");
	return result;
}

/// <summary>根据m1的内容,反向填充m2</summary>
static void Fill(Dictionary<string, HashSet<string>> m1, Dictionary<string, HashSet<string>> m2)
{
	foreach (var m in m1)
	{
		foreach (var n in m.Value)
		{
			if (!m2.TryGetValue(n, out var s))
			{
				Console.WriteLine($"获取失败: {n}");
				continue;
			}

			//Console.WriteLine($"{m} {n}");
			s.Add(m.Key);
		}
	}
	
	// 打印一下
	foreach (var r in m2)
	{
		var x = r.Key;
		var y = string.Join("\n", r.Value);
		Console.WriteLine($"{x}: {y}");
	}
}

static void Write(Dictionary<string, HashSet<string>> map, string path)
{   // 创建工作簿 (.xlsx)
	using var workbook = new XSSFWorkbook();
	// 获取工作表
	var sheet = workbook.CreateSheet();

	var cellStyle = workbook.CreateCellStyle();
	SetCellStyle(cellStyle);

	// 表头
	{
		var row = sheet.CreateRow(0);
		{
			var c = row.CreateCell(0);
			c.CellStyle = cellStyle;
			c.SetCellValue("序号");
		}
		{
			var c = row.CreateCell(1);
			c.CellStyle = cellStyle;
			c.SetCellValue("上级文档中被追踪内容");
		}
		{
			var c = row.CreateCell(2);
			c.CellStyle = cellStyle;
			c.SetCellValue("本文档中被追踪内容");
		}
		{
			var c = row.CreateCell(3);
			c.CellStyle = cellStyle;
			c.SetCellValue("备注");
		}
	}
	{
		var row = sheet.CreateRow(1);
		{
			var c = row.CreateCell(0);
			c.CellStyle = cellStyle;
		}
		{
			var c = row.CreateCell(1);
			c.CellStyle = cellStyle;
			c.SetCellValue("功能（名称/标识/章节）\n子功能（名称 / 标识）");
		}
		{
			var c = row.CreateCell(2);
			c.CellStyle = cellStyle;
			c.SetCellValue("对应功能需求\n（名称 / 章节）");
		}
		{
			var c = row.CreateCell(3);
			c.CellStyle = cellStyle;
		}
	}

	// 内容
	var r = 2;
	foreach (var m in map)
	{
		if (string.IsNullOrEmpty(m.Key) || m.Key=="-") 
		{
			continue;
		}
		
		var row = sheet.CreateRow(r++);
		{
			var c = row.CreateCell(0);
			c.CellStyle = cellStyle;
			c.SetCellValue(r-2);
		}
		{
			var c = row.CreateCell(1);
			c.CellStyle = cellStyle;
			c.SetCellValue(m.Key);
		}
		{
			var c = row.CreateCell(2);
			c.CellStyle = cellStyle;
			if (m.Value.Count == 0)
			{
				c.SetCellValue("-");
			}
			else
			{
				c.SetCellValue(string.Join("\n", m.Value));
			}
		}
		{
			var c = row.CreateCell(3);
			c.CellStyle = cellStyle;
			c.SetCellValue("-");
		}
	}

	// 写入文件
	File.Delete(path);
	using var ofs = new FileStream(path, FileMode.Create, FileAccess.Write);
	workbook.Write(ofs);
}


static KeyValuePair<string, HashSet<string>>? GetData(IRow row)
{
	if (row == null)
	{
		return null;
	}
	
	var index = row.GetCell(0);
	var cell1 = row.GetCell(1);
	var cell2 = row.GetCell(2);
	// cell1是否有值
	var b1 = cell1 != null && cell1.ToString().Trim().Length > 0;
	// cell2是否有值
	var b2 = cell2 != null && cell2.ToString().Trim().Length > 0;
	
	if (!b1 && !b2) 
	{
		return null;
	}
	
	// cell1有值, cell2没值, 说明没有对应,也应该返回结果
	else if (b1 && !b2)
	{
		return new KeyValuePair<string, HashSet<string>>(cell1?.ToString(), new HashSet<string>());
	}
	
	// cell1没值, cell2有值, 说明是同一格的继续
	else if (!b1 && b2)
	{
		// 获取被追踪项
		var set = new HashSet<string>();
		string[] items = cell2.ToString().Split('\n');
		foreach (string item in items)
		{
			set.Add(item.Trim());
		}
		return new KeyValuePair<string, HashSet<string>>(string.Empty, set);
	}

	else if (b1 && b2)
	{
		// 获取被追踪项
		var set = new HashSet<string>();
		string[] items = cell2.ToString().Split('\n');
		foreach (string item in items)
		{
			set.Add(item.Trim());
		}
		return new KeyValuePair<string, HashSet<string>>(cell1?.ToString().Trim(), set);
	}
	else
	{
		return null;
	}
}

/// <summary>打印表头</summary>
static string stringOf(IRow row)
{
	var sb = new StringBuilder();
	for (int cellIdx = 0; cellIdx < row.LastCellNum; cellIdx++)
	{
		// 获取当前单元格
		var cell = row.GetCell(cellIdx);
		// 确保单元格不为空
		if (cell == null)
		{
			continue;
		}
		sb.Append(cell.ToString());
		sb.Append("\t");
	}
	return sb.ToString();
}


static void SetCellStyle(ICellStyle cellStyle)
{
	// 设置边框
	cellStyle.BorderTop = BorderStyle.Thin;
	cellStyle.BorderBottom = BorderStyle.Thin;
	cellStyle.BorderLeft = BorderStyle.Thin;
	cellStyle.BorderRight = BorderStyle.Thin;

	// 设置边框颜色
	cellStyle.TopBorderColor = HSSFColor.Black.Index;
	cellStyle.BottomBorderColor = HSSFColor.Black.Index;
	cellStyle.LeftBorderColor = HSSFColor.Black.Index;
	cellStyle.RightBorderColor = HSSFColor.Black.Index;
}