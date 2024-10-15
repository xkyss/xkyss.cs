<Query Kind="Program">
  <NuGetReference>NPOI</NuGetReference>
  <Namespace>NPOI.XSSF.UserModel</Namespace>
  <Namespace>NPOI.SS.UserModel</Namespace>
  <Namespace>NPOI.HSSF.Util</Namespace>
</Query>

void Main()
{
	var filePath = @"D:\Code\zyst\Yfty\docs\06.数据\2024.09-休闲场所\2024.09.26\xxcsry0926.xlsx";
	var fixedPath = @"D:\Code\zyst\Yfty\docs\06.数据\2024.09-休闲场所\2024.09.26\xxcsry0926-fixed.xlsx";
	using var ifs = new FileStream(filePath, FileMode.Open, FileAccess.Read);

	// 创建工作簿 (.xlsx)
	using var workbook = new XSSFWorkbook(ifs);
	var cellStyle = workbook.CreateCellStyle();
	SetCellStyle(cellStyle);

	const int idIndex = 9;
	foreach (var sheetIdx in Enumerable.Range(0, 10))
	{
		// 获取工作表
		var sheet = workbook.GetSheetAt(sheetIdx);

		// 在第一行添加列名 "id"
		var headerRow = sheet.GetRow(0) ?? sheet.CreateRow(0);
		// 在第一列添加单元格
		var idCell = headerRow.CreateCell(idIndex);
		idCell.SetCellValue("id");
		idCell.CellStyle = cellStyle;

		// 遍历所有行，在每行的第一列添加 "id" 值
		for (int i = 1; i <= sheet.LastRowNum; i++)
		{
			var row = sheet.GetRow(i) ?? sheet.CreateRow(i);
			var cell = row.CreateCell(idIndex);
			cell.SetCellValue(Guid.NewGuid().ToString("N"));
			cell.CellStyle = cellStyle;
		}
	}


	// 写入文件
	using var ofs = new FileStream(fixedPath, FileMode.Create, FileAccess.Write);
	workbook.Write(ofs);
	
	Console.WriteLine("done");
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
