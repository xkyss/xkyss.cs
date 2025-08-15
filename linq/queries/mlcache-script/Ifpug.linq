<Query Kind="Program">
  <NuGetReference>NPOI</NuGetReference>
  <NuGetReference>SixLabors.ImageSharp</NuGetReference>
  <Namespace>NPOI.XSSF.UserModel</Namespace>
  <Namespace>NPOI.SS.UserModel</Namespace>
  <Namespace>NPOI.SS.Util</Namespace>
  <Namespace>NPOI.HSSF.Util</Namespace>
</Query>


//#!/usr/bin/dotnet run

// 过滤未匹配到的Ex-Ptr
// lprun8-x64.exe .\Ifpug.linq | Select-String -Pattern "NOT Exist|Sheet: "

void Main()
{
	
	var path1 = @"D:\Code\thzt\mlcache-doc\doc\07.work\05.功能点统计.xlsx";
	var path2 = @"D:\Code\thzt\mlcache-doc\doc\07.work\05.功能点统计-Ex.xlsx";
	var path3 = @"D:\Code\thzt\mlcache-doc\doc\09.part\01.追溯表目录.xlsx";


	// 读取文件
	using var fsIn = new FileStream(path1, FileMode.Open, FileAccess.Read);
	// 创建工作簿 (.xlsx)
	var workbookIn = new XSSFWorkbook(fsIn);


	// 创建工作簿 (.xlsx)
	using var workbookOut = new XSSFWorkbook();
	var cellStyle = workbookOut.CreateCellStyle();
	SetCellStyle(cellStyle);

	// 读取需归追溯目录
	var xgList = ReadXg(path3);

	// sheet1
	var sheet1 = workbookOut.CreateSheet();
	WriteHeader(sheet1, cellStyle);

	// sheet2
	var sheet2 = workbookOut.CreateSheet();

	// 创建标题行（首行）
	var headerRow = sheet2.CreateRow(0);
	headerRow.CreateCell(0).SetCellValue("序号").With(cellStyle);
	headerRow.CreateCell(1).SetCellValue("需归").With(cellStyle);
	headerRow.CreateCell(2).SetCellValue("功能点").With(cellStyle);
	// 序号 需归
	for (var i = 0; i < xgList.Count(); i++)
	{
		var xg = xgList[i];
		var row = sheet2.CreateRow(i + 1);
		row.CreateCell(0).SetCellValue($"{i + 1}").With(cellStyle);
		row.CreateCell(1).SetCellValue(xg).With(cellStyle);
	}

	for (var i = 2; i <= 7; i++)
	{
		// 获取工作表
		var sheetIn = workbookIn.GetSheetAt(i);
		var ifpug = Read(sheetIn);
		Write(sheet1, cellStyle, ifpug);

		WriteXgZsb(sheet2, cellStyle, ifpug, xgList);
	}

	var csRed = workbookOut.CreateCellStyle();
	csRed.CloneStyleFrom(cellStyle);
	csRed.FillForegroundColor = HSSFColor.Red.Index;
	csRed.FillPattern = FillPattern.SolidForeground;
	WriteZero(sheet1, csRed);
	
	SetColumnWidth(sheet1);

	// 保存到文件
	using var fs = new FileStream(path2, FileMode.Create, FileAccess.Write);
	workbookOut.Write(fs);
}

List<string> ReadXg(string path)
{
	// 读取文件
	using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
	// 创建工作簿 (.xlsx)
	var wb = new XSSFWorkbook(fs);
	var sheet = wb.GetSheetAt(0);
	// 记录已经处理过的合并区域（避免重复输出）
	var mergedRegions = new HashSet<CellRangeAddress>();

	Console.WriteLine();
	Console.WriteLine("------------- ReadXg");
	Console.WriteLine($"Sheet: {sheet.SheetName}");

	var ret = new List<string>();
	for (var rowIndex = 1; rowIndex <= sheet.LastRowNum; rowIndex++)
	{
		var row = sheet.GetRow(rowIndex);
		// 跳过空行
		if (row == null)
		{
			continue;
		}

		// 获取单元格的值
		var (mr, cv) = GetMergedCellValue(sheet, rowIndex, 0, mergedRegions);
		cv = cv?.Trim();
		if (string.IsNullOrEmpty(cv))
		{
			continue;
		}

		var xg = GetCellValueByIndex(sheet, rowIndex, 1)?.ToString();
		Console.WriteLine($"{rowIndex + 1}: {cv}: {xg}");
		ret.Add(xg);
	}
	
	return ret;
}

void WriteXgZsb(ISheet sheet, ICellStyle cellStyle, IfpugInfo ifpug, List<string> xgList)
{
	// FTR
	foreach (FtrInfo ftr in ifpug.Ftrs)
	{
		foreach (string xgInFtr in ftr.Xg)
		{
			var matched = false;
			for (var i = 0; i < xgList.Count(); i++)
			{
				if (xgList[i] == xgInFtr)
				{
					var row = sheet.GetRow(i+1);
					if (row == null)
					{
						row = sheet.CreateRow(i+1);
					}

					var cell = row?.GetCell(2);
					if (cell == null)
					{
						cell = row?.CreateCell(2);
					}

					var v = GetCellValue(cell);
					var v1 = $"{ftr.SheetName}-{ftr.Type}-{ftr.Name}";
					var v2 = string.IsNullOrEmpty(v) ? v1 : $"{v}\n{v1}";
					cell.SetCellValue(v2).With(cellStyle);
					matched = true;
					Console.WriteLine($"SET: ({i}, 2): {v2}");
				}
			}
			if (!matched)
			{
				Console.WriteLine($"NO match: {ftr.SheetName}({ftr.Content}): {xgInFtr}");
			}
		}
	}

	// EX
	foreach (ExInfo ex in ifpug.Exs)
	{
		foreach (string xgInEx in ex.Xg)
		{
			var matched = false;
			for (var i = 0; i < xgList.Count(); i++)
			{
				if (xgList[i] == xgInEx)
				{
					var row = sheet.GetRow(i + 1);
					if (row == null)
					{
						row = sheet.CreateRow(i + 1);
					}

					var cell = row?.GetCell(2);
					if (cell == null)
					{
						cell = row?.CreateCell(2);
					}

					var v = GetCellValue(cell);
					var v1 = $"{ex.SheetName}-{ex.Type}-{ex.Name}";
					var v2 = string.IsNullOrEmpty(v) ? v1 : $"{v}\n{v1}";
					cell.SetCellValue(v2).With(cellStyle);
					matched = true;
					Console.WriteLine($"SET: ({i}, 2): {v2}");
				}
			}
			if (!matched)
			{
				Console.WriteLine($"NO match: {ex.SheetName}({ex.Content}): {xgInEx}");
			}
		}
	}
}

IfpugInfo Read(ISheet sheet)
{
	var ret = new IfpugInfo();

	Console.WriteLine();
	Console.WriteLine("-------------");
	Console.WriteLine($"Sheet: {sheet.SheetName}");

	// 记录已经处理过的合并区域（避免重复输出）
	var mergedRegions = new HashSet<CellRangeAddress>();

	var ftrs = new Dictionary<string, FtrInfo>();
	// 遍历每一行 (略过标题)
	int rowIndex = 1;
	
	// 读取FTR
	for (; rowIndex <= sheet.LastRowNum; rowIndex++)
	{
		var row = sheet.GetRow(rowIndex);
		// 跳过空行
		if (row == null)
		{
			continue;
		}

		// 获取单元格的值
		var (mr, cv) = GetMergedCellValue(sheet, rowIndex, 0, mergedRegions);
		cv = cv?.Trim();
		// 如果不是合并单元格, 表示当前表格已经结束
		if (!mr && string.IsNullOrEmpty(cv))
		{
			break;
		}
		// 如果值为null, 表示是被合并的单元, 不输出
		if (string.IsNullOrEmpty(cv))
		{
			continue;
		}

		var info = new FtrInfo()
		{
			LineNumber = rowIndex + 1,
			Content = cv,
			Name = new string(cv.TakeWhile(c => c != '(').ToArray()).Trim(),
			SheetName = sheet.SheetName.Trim(),
			Rs = GetCellValueByIndex(sheet, rowIndex, 5)?.ToListString(), 	// F列, 软件设计文档
			Xg = GetCellValueByIndex(sheet, rowIndex, 6)?.ToListString(), 	// G列, 需求规格文档
			Cs = GetCellValueByIndex(sheet, rowIndex, 7)?.ToListString(), 	// H列, 测试用例
			Type = GetCellValueByIndex(sheet, rowIndex, 8)?.ToString(), // I列, 类型
		};

		if (ftrs.ContainsKey(info.Name))
		{
			throw new DuplicateNameException(info.Name);
		}

		Console.WriteLine($"{rowIndex + 1}: {info.Content}");
		ftrs.Add(info.Name, info);
	}

	// 读取EX
	var exs = new Dictionary<string, ExInfo>();
	var type = "";
	var lastEx = (ExInfo) null;
	for (; rowIndex <= sheet.LastRowNum; rowIndex++)
	{
		var row = sheet.GetRow(rowIndex);
		// 跳过空行
		if (row == null)
		{
			continue;
		}

		var (m0, c0) = GetMergedCellValue(sheet, rowIndex, 0, mergedRegions);
		c0 = c0?.Trim();
		
		// 判断开始
		if (string.IsNullOrEmpty(type))
		{
			if (c0 == "EO" || c0 == "EI" || c0 == "EQ")
			{
				type = c0;
				Console.WriteLine($"{rowIndex + 1}: Start {type}");
			}

			continue;
		}
		// 判断结束 (非合并格无值)
		if (!m0 && string.IsNullOrEmpty(c0))
		{
			Console.WriteLine($"{rowIndex + 1}: End {type}");
			type = "";
			continue;
		}

		if (!string.IsNullOrEmpty(c0))
		{
			var info = new ExInfo()
			{
				SheetName = sheet.SheetName,
				LineNumber = rowIndex + 1,
				Content = c0,
				Name = new string(c0.TakeWhile(c => c != '(').ToArray()).Trim(),
				Rs = GetCellValueByIndex(sheet, rowIndex, 5)?.ToListString(),   // F列, 软件设计文档
				Xg = GetCellValueByIndex(sheet, rowIndex, 6)?.ToListString(),   // G列, 需求规格文档
				Cs = GetCellValueByIndex(sheet, rowIndex, 7)?.ToListString(),   // H列, 测试用例
				Type = GetCellValueByIndex(sheet, rowIndex, 8)?.ToString(), // I列, 类型
				Ftrs = new List<FtrInfo>(),
			};
			lastEx = info;
			exs.Add(info.Name, info);
		}


		// 读取Ex-Ftr
		var (m1, c1) = GetMergedCellValue(sheet, rowIndex, 1, mergedRegions);
		c1 = c1?.Trim();
		if (string.IsNullOrEmpty(c1))
		{
			continue;
		}


		if (!ftrs.TryGetValue(c1, out var ftr))
		{
			//throw new Exception($"FTR in {type} NOT exist. {c1}");
			Console.WriteLine($"{rowIndex + 1}: FTR in {type} NOT exist: {c1}");
			continue;
		}

		Console.WriteLine($"{rowIndex + 1}: {lastEx.Content} {ftr.Content}");
		lastEx.Ftrs.Add(ftr);
	}

	ret.Ftrs = ftrs.Values.ToList();
	ret.Exs = exs.Values.ToList();
	return ret;
}

void WriteHeader(ISheet sheet, ICellStyle cellStyle)
{
	// 创建标题行（首行）
	var headerRow = sheet.CreateRow(0);
	headerRow.CreateCell(0).SetCellValue("模块").With(cellStyle);
	headerRow.CreateCell(1).SetCellValue("行号").With(cellStyle);
	headerRow.CreateCell(2).SetCellValue("ILF/EIF").With(cellStyle);
	headerRow.CreateCell(3).SetCellValue("EI").With(cellStyle);
	headerRow.CreateCell(4).SetCellValue("EO").With(cellStyle);
	headerRow.CreateCell(5).SetCellValue("EQ").With(cellStyle);
}

void SetColumnWidth(ISheet sheet)
{
	// 设置A到F列的宽度, 单位：字符
	int[] widths = { 14, 4, 40, 8, 5, 5 }; 
	for (int i = 0; i < widths.Length; i++)
	{
		sheet.SetColumnWidth(i, widths[i] * 256);
	}
}

void Write(ISheet sheet, ICellStyle cellStyle, IfpugInfo ifpug)
{
	var start = sheet.LastRowNum;
	// 填充数据行
	var ftrs = ifpug.Ftrs;
	var exs = ifpug.Exs;
	for (var i = 0; i < ftrs.Count; i++)
	{
		var ftr = ftrs[i];
		var row = sheet.CreateRow(i + start + 1);

		row.CreateCell(0).SetCellValue(ftr.SheetName).With(cellStyle);
		row.CreateCell(1).SetCellValue(ftr.LineNumber).With(cellStyle);
		row.CreateCell(2).SetCellValue(ftr.Content).With(cellStyle);
		row.CreateCell(3).SetCellValue(GetScore(ftr, exs, "EI")).With(cellStyle);
		row.CreateCell(4).SetCellValue(GetScore(ftr, exs, "EO")).With(cellStyle);
		row.CreateCell(5).SetCellValue(GetScore(ftr, exs, "EQ")).With(cellStyle);
	}
}

void WriteZero(ISheet sheet, ICellStyle cellStyle)
{
	for (var rowIndex = 1; rowIndex < sheet.LastRowNum; rowIndex++)
	{
		var e3 = GetCellValueByIndex(sheet, rowIndex, 3);
		var e4 = GetCellValueByIndex(sheet, rowIndex, 4);
		var e5 = GetCellValueByIndex(sheet, rowIndex, 5);

		var zero = e3 == "0" && e4 == "0" && e5 == "0";
		Console.WriteLine($"{e3} {e4} {e5} {zero}");
		if (zero)
		{
			var row = sheet.GetRow(rowIndex);
			row.GetCell(2).With(cellStyle);
		}
	}
}

double GetScore(FtrInfo ftr, List<ExInfo> exs, string exType)
{
	if (exs == null || exs.Count() == 0)
	{
		return 0;
	}
	
	double score = 0;
	foreach (var ex in exs)
	{
		var ftrs = ex.Ftrs;
		if (ftrs == null || ftrs.Count() == 0)
		{
			continue;
		}
		if (ex.Type != exType)
		{
			continue;
		}
		
		if (ftrs.Any(f => f.Name == ftr.Name))
		{
			score += (1.0/ftrs.Count());
		}
	}

	Console.WriteLine($"Score of {exType} {ftr.Name}: {score}");
	return score;
}


// 获取合并单元格的值（并记录已处理的合并区域）
static (bool isInMergeRegion, string value) GetMergedCellValue(ISheet sheet, int rowIndex, int colIndex, HashSet<CellRangeAddress> processedMergedRegions)
{
	if (IsInMergedRegion(sheet, rowIndex, colIndex, out var mergedRegion))
	{
		// 如果这个合并区域已经处理过，则返回空字符串
		if (processedMergedRegions.Contains(mergedRegion))
		{
			return (true, null);  // 已经输出过，不再重复输出
		}

		// 否则，记录该合并区域，并返回左上角单元格的值
		processedMergedRegions.Add(mergedRegion);
		return (true, GetCellValueByIndex(sheet, mergedRegion.FirstRow, mergedRegion.FirstColumn));
	}
	else
	{
		// 如果不是合并单元格，正常获取值
		return (false, GetCellValueByIndex(sheet, rowIndex, colIndex));
	}
}

// 检查单元格是否在合并区域内
static bool IsInMergedRegion(ISheet sheet, int rowIndex, int colIndex, out CellRangeAddress mergedRegion)
{
	foreach (var region in sheet.MergedRegions)
	{
		if (rowIndex >= region.FirstRow &&
			rowIndex <= region.LastRow &&
			colIndex >= region.FirstColumn &&
			colIndex <= region.LastColumn)
		{
			mergedRegion = region;
			return true;
		}
	}
	mergedRegion = null;
	return false;
}

// 获取单元格的值（支持数字、字符串、布尔值、公式等）
static string GetCellValue(ICell cell)
{
	if (cell == null)
	{
		return null;
	}

	switch (cell.CellType)
	{
		case CellType.String:
			return cell.StringCellValue;
		case CellType.Numeric:
			return DateUtil.IsCellDateFormatted(cell)
				? cell.DateCellValue.ToString()
				: cell.NumericCellValue.ToString();
		case CellType.Boolean:
			return cell.BooleanCellValue.ToString();
		case CellType.Formula:
			return GetCellValue(cell);  // 递归处理公式结果
		default:
			return "";
	}
}

static string GetCellValueByIndex(ISheet sheet, int rowIndex, int colIndex)
{
	var row = sheet.GetRow(rowIndex);
	var cell = row?.GetCell(colIndex);
	return GetCellValue(cell);
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

public static class NpoiExtensions
{
	// 单元格样式扩展方法
	public static ICell With(this ICell cell, ICellStyle style)
	{
		cell.CellStyle = style;
		return cell;
	}

	// 便捷创建带样式的单元格（可选）
	public static ICell CreateCellWithStyle(this IRow row, int column, ICellStyle style)
	{
		var cell = row.CreateCell(column);
		cell.CellStyle = style;
		return cell;
	}
}

public static class StringExtensions
{
	public static List<string> ToListString(this string @this)
	{
		if (string.IsNullOrEmpty(@this))
		{
			return new List<string>();
		}

		// 使用 Split 方法按换行符分割字符串
		// 移除空条目，并修剪每行的前后空白
		return @this.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
				   .Select(line => line.Trim())
				   .Where(line => !string.IsNullOrEmpty(line))
				   .ToList();
	}
}

class FtrInfo
{
	/// <summary>简化内容,移除Content中的括号,英文名等</summary>
	public string Name { get; set; }
	
	/// <summary>原始内容</summary>
	public string Content { get; set; }

	/// <summary>软设追溯</summary>
	public List<string> Rs { get; set; }

	/// <summary>需归追溯</summary>
	public List<string> Xg { get; set; }
	
	/// <summary>测试追溯</summary>
	public List<string> Cs { get; set; }

	/// <summary>类型: ILF/EIL</summary>
	public string Type {get;set;}
	
	/// <summary>Excel-Sheet表名</summary>
	public string SheetName { get; set; }

	/// <summary>行号,如果是合并单元格, 则为合并的第一行</summary>
	public int LineNumber { get; set; }
}

class ExInfo
{
	/// <summary>简化内容,移除Content中的括号,英文名等</summary>
	public string Name { get; set; }

	/// <summary>原始内容</summary>
	public string Content { get; set; }

	/// <summary>软设追溯</summary>
	public List<string> Rs { get; set; }

	/// <summary>需归追溯</summary>
	public List<string> Xg { get; set; }

	/// <summary>测试追溯</summary>
	public List<string> Cs { get; set; }

	/// <summary>类型: EI/EO/EQ</summary>
	public string Type { get; set; }
	
	/// <summary>Excel-Sheet表名</summary>
	public string SheetName { get; set; }

	/// <summary>行号,如果是合并单元格, 则为合并的第一行</summary>
	public int LineNumber { get; set; }

	/// <summary>包含的FTR信息</summary>
	public List<FtrInfo> Ftrs { get; set; }
}

class IfpugInfo
{
	/// <summary>FTR 列表</summary>
	public List<FtrInfo> Ftrs { get; set; }
	
	/// <summary>EX 列表</summary>
	public List<ExInfo> Exs { get; set; }
}
