<Query Kind="Program">
  <NuGetReference>NPOI</NuGetReference>
  <Namespace>NPOI.XSSF.UserModel</Namespace>
  <Namespace>NPOI.SS.UserModel</Namespace>
</Query>

void Main()
{
	var fixedPath = @"E:\xk\Code\zyaj\jwt_v3\ydjwv3\trunk\working\gits\Yfty\docs\2024.09.13-休闲场所\休闲\休闲场所从业人员表-fixed.xlsx";
	using var fs = new FileStream(fixedPath, FileMode.Open, FileAccess.Read);

	// 创建工作簿 (.xlsx)
	var workbook = new XSSFWorkbook(fs);
	// 获取工作表
	var sheet = workbook.GetSheetAt(0);

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
			var sql = sqlOf(row);
			if (sql != null)
			{
				Console.WriteLine(sql);
			}
		}
	}

	Console.WriteLine(UpdateCsdm);
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
static string sqlOf(IRow row)
{
	count++;

	var uuid = row.GetCell(10).ToString();

	var sb = new StringBuilder();
	sb.Append(@"REPLACE INTO t_cyry (OBJ_ID, CSMC, CSDM, CYRY_XM, CYRY_ZJHM, CYRY_LXDH, YLTH_GWMC, CREATE_TIME, IS_UPLOAD, DATA_SOURCES, SJC, OPT_STATE, STATE, BZ2) VALUES ");
	sb.Append("(");

	// OBJ_ID
	sb.Append($"'{uuid}', ");
	// CSMC
	sb.Append($"'{row.GetCell(3).ToString()}', ");
	// CSDM, 先用CSMC代替
	sb.Append($"'{row.GetCell(3).ToString()}', ");
	// CYRY_XM
	sb.Append($"'{row.GetCell(4).ToString()}', ");
	// CYRY_ZJHM
	sb.Append($"'{row.GetCell(8).ToString()}', ");
	// CYRY_LXDH
	sb.Append($"'{row.GetCell(9).ToString()}', ");
	// YLTH_GWMC
	sb.Append($"'{row.GetCell(7).ToString()}', ");
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
	sb.Append($"'{row.GetCell(5).ToString()}'");

	sb.Append(");");
	return sb.ToString();
}

static string UpdateCsdm = """
-- 更新CSDM, 如果没找到对应的场所, 会保留
UPDATE t_cyry ry
SET CSDM = IF(
    EXISTS (
        SELECT 1
        FROM t_csxx
        WHERE CSMC=ry.CSMC
    ),
    (
        SELECT CSDM
        FROM t_csxx
        WHERE CSMC=ry.CSMC
        LIMIT 1
    ),
    CSDM
)
WHERE DATA_SOURCES = '20';
""";
