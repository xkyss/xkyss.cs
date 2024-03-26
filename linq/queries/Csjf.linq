<Query Kind="Program">
  <NuGetReference>NPOI</NuGetReference>
  <Namespace>NPOI.XSSF.UserModel</Namespace>
  <Namespace>NPOI.SS.UserModel</Namespace>
</Query>

void Main()
{
	var filePath = @"E:\xk\Code\zyaj\jwt_v3\ydjwv3\trunk\working\gits\Yfty\docs\2024.03.21-场所积分\2024.03.26-王君-积分数据表格.xlsx";
	using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);

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
		if (row == null) {
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
static string sqlOf(IRow row) 
{
	count++;
	//Console.WriteLine($"{count} {row.LastCellNum}");
	if (row.LastCellNum != 21)
	{
		//Console.WriteLine("-------------");
		return null;
	}
	var sb = new StringBuilder();
	sb.Append(@"INSERT INTO t_csjf (`rq`, `hylx`, `tzhylbdm`, `dwmc`, `dwbm`, `ssjybm`, `csmc`, `csdm`, `csdz`, `cyry_xm`, `cyry_zjhm`, `lxdh`, `qymc`, `qydm`, `zfsz`, `bcfz`, `jfyy`, `cfyy`, `zxfz`, `pm`, `bz`) VALUES ");
	sb.Append("(");
	for (int cellIdx = 0; cellIdx < row.LastCellNum; cellIdx++)
	{
		// 获取当前单元格
		var cell = row.GetCell(cellIdx);
		// 确保单元格不为空
		if (cell == null)
		{
			continue;
		}
		sb.Append($"'{cell.ToString()}'");
		if (cellIdx < row.LastCellNum - 1) 
		{
			sb.Append(",");
		}
	}
	
	sb.Append(");");
	return sb.ToString();
}
