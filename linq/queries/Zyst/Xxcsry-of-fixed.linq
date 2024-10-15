<Query Kind="Program">
  <NuGetReference>NPOI</NuGetReference>
  <Namespace>NPOI.XSSF.UserModel</Namespace>
  <Namespace>NPOI.SS.UserModel</Namespace>
</Query>

void Main()
{
	// 读取场所信息,主要是csdm
	var csxxFixedPath = @"D:\Code\zyst\Yfty\docs\06.数据\2024.09-休闲场所\2024.09.23-休闲场所\xxzb0924-fixed.xlsx";
	var csDic = new Dictionary<string, string>();
	{
		using var csfs = new FileStream(csxxFixedPath, FileMode.Open, FileAccess.Read);
		var csSheet = new XSSFWorkbook(csfs).GetSheetAt(0);

		// 遍历工作表中的每一行
		for (int rowIdx = 0; rowIdx <= csSheet.LastRowNum; rowIdx++)
		{
			// 获取当前行
			var row = csSheet.GetRow(rowIdx);
			// 确保行不为空
			if (row == null)
			{
				continue;
			}

			// 表头
			if (rowIdx == 0 || rowIdx == 1)
			{
				//Console.WriteLine(stringOf(row));
			}
			else
			{
				//Console.Write(row.GetCell(3).ToString());
				//Console.Write(", ");
				//Console.WriteLine(row.GetCell(7).ToString());
				var name = $"{row.GetCell(3).ToString()}-{row.GetCell(2).ToString()}";
				var uuid = row.GetCell(7).ToString();
				if (!csDic.TryAdd(name, uuid))
				{
					//Console.WriteLine($"  重复场所: {name}");
				}
			}
		}
	}

	// 生成sql
	var fixedPath = @"D:\Code\zyst\Yfty\docs\06.数据\2024.09-休闲场所\2024.09.26\xxcsry0926-fixed.xlsx";
	using var fs = new FileStream(fixedPath, FileMode.Open, FileAccess.Read);

	// 创建工作簿 (.xlsx)
	var workbook = new XSSFWorkbook(fs);

	foreach (var sheetIdx in Enumerable.Range(0, 10))
	{
		// 获取工作表
		var sheet = workbook.GetSheetAt(sheetIdx);

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
				var sql = sqlOf(row, csDic);
				if (sql != null)
				{
					Console.WriteLine(sql);
				}
			}
		}
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


static int count = 0;
static string dataFormat = "yyyy-MM-dd HH:mm:ss";
static string sqlOf(IRow row, Dictionary<string, string> csDic)
{
	count++;

	var uuid = row.GetCell(9).ToString();

	var sb = new StringBuilder();
	sb.Append(@"REPLACE INTO t_cyry (OBJ_ID, CSMC, CSDM, CYRY_XM, CYRY_ZJHM, CYRY_LXDH, YLTH_GWMC, CREATE_TIME, IS_UPLOAD, DATA_SOURCES, SJC, OPT_STATE, STATE, BZ2) VALUES ");
	sb.Append("(");

	// OBJ_ID
	sb.Append($"'{uuid}', ");
	// CSMC
	sb.Append($"'{row.GetCell(2)?.ToString()}', ");
	// CSDM
	if (IsValid(row.GetCell(2)) && IsValid(row.GetCell(1)))
	{
		var name = $"{row.GetCell(2).ToString()}-{row.GetCell(1).ToString()}";
		if (csDic.TryGetValue(name, out var csUuid))
		{
			sb.Append($"'{csUuid}', ");
		}
		else
		{
			sb.Append($"'{row.GetCell(2)?.ToString()}', ");
		}
	}
	else
	{
		sb.Append($"'{row.GetCell(2)?.ToString()}', ");
	}
	// CYRY_XM
	sb.Append($"'{row.GetCell(3)?.ToString()}', ");
	// CYRY_ZJHM
	sb.Append($"'{row.GetCell(7)?.ToString().Replace("'", "")}', ");
	// CYRY_LXDH
	sb.Append($"'{row.GetCell(8)?.ToString()}', ");
	// YLTH_GWMC
	sb.Append($"'{row.GetCell(6)?.ToString()}', ");
	// CREATE_TIME
	sb.Append($"'{DateTime.Now.ToString(dataFormat)}', ");
	// IS_UPLOAD
	sb.Append($"0, ");
	// DATA_SOURCES
	sb.Append($"'20', ");
	// SJC
	sb.Append($"{new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds()}, ");
	// OPT_STATE
	sb.Append($"1, ");
	// STATE
	sb.Append($"1, ");
	// BZ2
	sb.Append($"'{row.GetCell(5)?.ToString()}'");

	sb.Append(");");
	return sb.ToString();
}

static bool IsValid(ICell cell) 
{
	if (cell == null) 
	{
		return false;
	}
	
	if (string.IsNullOrWhiteSpace(cell.ToString())) {
		return false;
	}
	
	return true;
}
