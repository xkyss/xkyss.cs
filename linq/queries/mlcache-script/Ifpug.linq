<Query Kind="Statements">
  <NuGetReference>NPOI</NuGetReference>
  <NuGetReference>SixLabors.ImageSharp</NuGetReference>
  <Namespace>NPOI.XSSF.UserModel</Namespace>
  <Namespace>NPOI.SS.UserModel</Namespace>
  <Namespace>NPOI.SS.Util</Namespace>
  <Namespace>NPOI.HSSF.Util</Namespace>
</Query>

//#!/usr/bin/dotnet run

var path1 = @"D:\Code\thzt\mlcache-doc\doc\07.work\05.功能点统计.xlsx";
var path2 = @"D:\Code\thzt\mlcache-doc\doc\07.work\05.功能点统计-Ex.xlsx";

var ftrs = ReadFtrs(path1);
WriteFtrs(path2, ftrs);


List<FtrInfo> ReadFtrs(string path)
{
	using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);

	// 记录已经处理过的合并区域（避免重复输出）
	var processedMergedRegions = new HashSet<CellRangeAddress>();

	// 创建工作簿 (.xlsx)
	var workbook = new XSSFWorkbook(fs);

	var ftrList = new List<FtrInfo>();
	for (var sheetIndex = 1; sheetIndex <= 6; sheetIndex++)
	{
		// 获取工作表
		var sheet = workbook.GetSheetAt(sheetIndex);

		Console.WriteLine();
		Console.WriteLine("----------------------------");
		Console.WriteLine($"Sheet {sheetIndex} Title: " + sheet.SheetName);
		var ftrs = ReadFtr(sheet, processedMergedRegions);
		foreach (var ftr in ftrs) 
		{
			ftr.SheetIndex = sheetIndex;
			ftr.SheetName = sheet.SheetName;
		}
		ftrList.AddRange(ftrs);
	}

	Console.WriteLine();
	Console.WriteLine("----------------------------");
	Console.WriteLine($"Ftr count: {ftrList.Count}");
	
	return ftrList;
	
}

// 获得FTR信息
List<FtrInfo> ReadFtr(ISheet sheet, HashSet<CellRangeAddress> processedMergedRegions)
{
	var ret = new List<FtrInfo>();
	var count = 0;
	// 遍历每一行 (略过标题)
	for (int rowIndex = 1; rowIndex <= sheet.LastRowNum; rowIndex++)
	{
		var row = sheet.GetRow(rowIndex);
		// 跳过空行
		if (row == null)
		{
			continue;
		}


		// 获取单元格的值
		var (isInMergeRegion, cellValue) = GetMergedCellValue(sheet, rowIndex, 0, processedMergedRegions);
		// 如果不是合并单元格, 表示当前表格已经结束
		if (!isInMergeRegion)
		{
			break;
		}
		// 如果值为null, 表示是被合并的单元, 不输出
		else
		{
			if (cellValue == null)
			{
				continue;
			}
		}

		count++;

		// 固定宽度4，右对齐
		Console.Write(string.Format("{0,2}({1,3}): ", count, rowIndex + 1));
		Console.Write($"{cellValue}");

		// 换行
		Console.WriteLine();
		ret.Add(new FtrInfo()
		{
			LineNumber = rowIndex + 1,
			Content = cellValue,
		});
	}
	
	return ret;
}

// 获取合并单元格的值（并记录已处理的合并区域）
(bool isInMergeRegion, string value) GetMergedCellValue(ISheet sheet, int rowIndex, int colIndex, HashSet<CellRangeAddress> processedMergedRegions)
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
        var firstRow = sheet.GetRow(mergedRegion.FirstRow);
        var firstCell = firstRow?.GetCell(mergedRegion.FirstColumn);
        return (true, GetCellValue(firstCell));
    }
    else
    {
        // 如果不是合并单元格，正常获取值
        var row = sheet.GetRow(rowIndex);
        var cell = row?.GetCell(colIndex);
        return (false, GetCellValue(cell));
    }
}

// 检查单元格是否在合并区域内
bool IsInMergedRegion(ISheet sheet, int rowIndex, int colIndex, out CellRangeAddress mergedRegion)
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
string GetCellValue(ICell cell)
{
    if (cell == null) return "";

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

void WriteFtrs(string path2, List<FtrInfo> ftrList)
{
	// 创建工作簿 (.xlsx)
	using var workbook = new XSSFWorkbook();
	var cellStyle = workbook.CreateCellStyle();
	SetCellStyle(cellStyle);

	// 创建工作表（默认名称或按需调整）
	var sheet = workbook.CreateSheet("功能点列表");

	// 创建标题行（首行）
	var headerRow = sheet.CreateRow(0);
	headerRow.CreateCell(0).SetCellValue("模块").With(cellStyle);
	headerRow.CreateCell(1).SetCellValue("行号").With(cellStyle);
	headerRow.CreateCell(2).SetCellValue("ILF/EIF").With(cellStyle);
	headerRow.CreateCell(3).SetCellValue("查询").With(cellStyle);
	headerRow.CreateCell(4).SetCellValue("增加").With(cellStyle);
	headerRow.CreateCell(5).SetCellValue("修改").With(cellStyle);
	headerRow.CreateCell(6).SetCellValue("查询").With(cellStyle);

	// 填充数据行
	for (int i = 0; i < ftrList.Count; i++)
	{
		var ftr = ftrList[i];
		var row = sheet.CreateRow(i + 1);  // 从第2行开始

		row.CreateCell(0).SetCellValue(ftr.SheetName).With(cellStyle);
		row.CreateCell(1).SetCellValue(ftr.LineNumber).With(cellStyle);
		row.CreateCell(2).SetCellValue(ftr.Content).With(cellStyle);
		row.CreateCell(3).With(cellStyle);
		row.CreateCell(4).With(cellStyle);
		row.CreateCell(5).With(cellStyle);
		row.CreateCell(6).With(cellStyle);
	}

	// 优化列宽（自动调整+最小宽度）
	for (int i = 0; i <= 6; i++)
	{
		sheet.AutoSizeColumn(i);
		// 设置最小列宽（单位：1/256个字符宽度）
		if (sheet.GetColumnWidth(i) < 2000)
		{
			sheet.SetColumnWidth(i, 2000);
		}
	}


	// 保存到文件
	using var fs = new FileStream(path2, FileMode.Create, FileAccess.Write);
	workbook.Write(fs);
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

class FtrInfo
{
	public int SheetIndex {get; set;}
	public string SheetName {get; set;}
	public int LineNumber {get; set;}
	public string Content { get; set; }
}
