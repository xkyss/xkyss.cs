<Query Kind="Program">
  <NuGetReference>NPOI</NuGetReference>
  <Namespace>NPOI.XSSF.UserModel</Namespace>
  <Namespace>NPOI.SS.UserModel</Namespace>
</Query>

/// <summary>提取类别代码</summary>
void Main()
{
	var fixedPath = @"E:\xk\Code\zyaj\jwt_v3\ydjwv3\trunk\working\gits\Yfty\docs\2024.09.23-休闲场所\xxzb0924-fixed.xlsx";
	using var fs = new FileStream(fixedPath, FileMode.Open, FileAccess.Read);
	
	// 创建工作簿 (.xlsx)
	var workbook = new XSSFWorkbook(fs);
	// 获取工作表
	var sheet = workbook.GetSheetAt(0);
	
	// 用Set保存出现了的类别
	HashSet<string> lbSet = new HashSet<string>();

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
		if (rowIdx == 0 || rowIdx == 1)
		{
			//Console.WriteLine(stringOf(row));
		}
		else
		{
			//Console.WriteLine(row.GetCell(5).ToString());
			lbSet.Add(row.GetCell(5).ToString().Trim());
		}
	}
	
	var i = 0;
	foreach(string lb in lbSet)
	{
		Console.WriteLine($"{lb}(\"{++i, 2:D2}\"),");
	}
}



static int count = 0;
static string dataFormat = "yyyy-MM-dd HH:mm:ss";
static string sqlOf(IRow row)
{
	count++;
	
	var uuid = row.GetCell(7).ToString();

	var sb = new StringBuilder();
	sb.Append(@"REPLACE INTO t_csxx (OBJ_ID, TZHYLBDM, CSMC, CSDM, ZAGLDWBM, CS_DZMC, CSFZR_XM, CREATE_TIME, IS_UPLOAD, DATA_SOURCES, SJC, OPT_STATE, BZ2, STATE) VALUES ");
	sb.Append("(");

	// OBJ_ID
	sb.Append($"'{uuid}', ");
	// TZHYLBDM
	sb.Append($"'20', ");
	// CSMC
	sb.Append($"'{row.GetCell(3).ToString()}', ");
	// CSDM
	sb.Append($"'XXCS-{uuid.Substring(0, 8)}', ");
	// ZAGLDWBM
	sb.Append($"'{row.GetCell(2).ToString()}', ");
	// CS_DZMC
	sb.Append($"'{row.GetCell(4).ToString()}', ");
	// CSFZR_XM
	sb.Append($"'{row.GetCell(6).ToString()}', ");
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
	// BZ2
	sb.Append($"'{row.GetCell(5).ToString()}', ");
	// STATE
	sb.Append($"1");

	sb.Append(");");
	return sb.ToString();
}

static string DeleteXxcs = """
DELETE FROM t_csxx WHERE TZHYLBDM='20';
""";

static string UpdateZagldwbm = """
-- 更新ZAGLDWBM, 如果没找到对应的部门, 会保留
UPDATE t_csxx cs
SET ZAGLDWBM = IF(
    EXISTS (
        SELECT 1
        FROM t_dept
        WHERE name LIKE CONCAT('%', cs.ZAGLDWBM)
    ),
    (
        SELECT code
        FROM t_dept
        WHERE name LIKE CONCAT('%', cs.ZAGLDWBM)
        LIMIT 1
    ),
    ZAGLDWBM
)
WHERE TZHYLBDM = '20';
""";
